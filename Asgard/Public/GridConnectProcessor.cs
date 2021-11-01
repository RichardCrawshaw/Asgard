using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Threading;
using NLog;

namespace Asgard
{
    /// <summary>
    /// Abstract class to handle the processing of GridConnect messages.
    /// </summary>
    public abstract class GridConnectProcessor :
        IGridConnectProcessor
    {
        #region Fields

        /// <summary>
        /// The start character of a GridConnect message.
        /// </summary>
        private const char MESSAGE_START = ':';

        /// <summary>
        /// The terminating character of a GridConnect message.
        /// </summary>
        private const char MESSAGE_TERMINATE = ';';

        /// <summary>
        /// Logger for standard program logging.
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="ISettings"/> object.
        /// </summary>
        private readonly ISettings settings;

        /// <summary>
        /// The comms adaptor that the messages are to be received via.
        /// </summary>
        private readonly ICommsAdapter commsAdapter;

        /// <summary>
        /// The queue that incoming messages are placed in so they can be handled in context of each
        /// other.
        /// </summary>
        private readonly ConcurrentQueue<string> receiveQueue = new();

        /// <summary>
        /// Used to manage the receiveQueue; flag whenever a message is received.
        /// </summary>
        private readonly ManualResetEventSlim manualResetEvent = new();

        /// <summary>
        /// Used to manage disconnecting the comms adapter.
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource = new();

        /// <summary>
        /// Used to manage disposing of the comms adapter.
        /// The flag will be reset when processing is started; it will be set when processing is
        /// complete. It is checked when disposing of the comms adapter.
        /// </summary>
        private readonly AutoResetEvent autoResetEvent = new(true);

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the current instance is connected.
        /// </summary>
        public bool IsConnected => this.commsAdapter.IsConnected;

        /// <summary>
        /// Gets whether the <see cref="GridConnectProcessor"/> has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="GridConnectProcessor"/> using the specified 
        /// <paramref name="commsAdapter"/>.
        /// </summary>
        /// <param name="commsAdapter">A <see cref="ICommsAdapter"/> object.</param>
        public GridConnectProcessor(ISettings settings, ICommsAdapter commsAdapter)
        {
            logger.Trace(() => nameof(GridConnectProcessor));

            if (commsAdapter is null)
                throw new ArgumentNullException(nameof(commsAdapter));

            this.settings = settings;
            this.commsAdapter = commsAdapter;
        }

        #endregion

        #region IDisposable support

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (this.IsConnected)
                    Disconnect();

                if (disposing)
                {
                    // dispose managed state (managed objects)

                    // Wait for the comms processing to complete, up to one second.
                    this.autoResetEvent.WaitOne(1000);
                    if (this.commsAdapter is not null)
                        this.commsAdapter.DataReceived -= CommsAdapter_DataReceived;
                    if (this.commsAdapter is ISerialPortAdapter serialPortAdapter)
                        serialPortAdapter.SerialPortError -= SerialPortAdapter_SerialPortError;

                    this.commsAdapter?.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                this.IsDisposed = true;
            }

            logger.Trace(() => $"{nameof(GridConnectProcessor)} has been disposed of.");
        }

        // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~GridConnectProcessor()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Connect the current instance to the specified <paramref name="portNumber"/>.
        /// </summary>
        /// <param name="portNumber">The number of the serial port to use.</param>
        /// <returns>True on success; false otherwise.</returns>
        public bool Connect()
        {
            logger.Trace(() => nameof(Connect));

            // Move this to a derived class.
            //if (this.commsAdapter is ISerialPortAdapter serialPortAdapter)
            //{
            //    serialPortAdapter.PortName = $"COM{portNumber}";
            //    serialPortAdapter.BaudRate = 115200;
            //    serialPortAdapter.DataBits = 8;
            //    serialPortAdapter.StopBits = StopBits.One;
            //    serialPortAdapter.Parity = Parity.None;
            //}

            this.commsAdapter.Connect();
            if (!this.commsAdapter.IsConnected) return false;

            this.commsAdapter.DataReceived += CommsAdapter_DataReceived;
            if (this.commsAdapter is ISerialPortAdapter serialPortAdapter)
                serialPortAdapter.SerialPortError += SerialPortAdapter_SerialPortError;

            logger.Debug(() => $"Connected to {this.commsAdapter.Name}");

            // Start the receive processing routine on a separate thread. This will run
            // independently until the Disconnect method is called.
            ThreadPool.QueueUserWorkItem(_ => ProcessSerial());

            return true;
        }

        /// <summary>
        /// Disconnect the current instance.
        /// </summary>
        /// <returns>True on success; false otherwise.</returns>
        public bool Disconnect()
        {
            logger.Trace(() => nameof(Disconnect));

            try
            {
                if (!this.commsAdapter.IsConnected)
                {
                    // Make sure the flag is set to allow disposal of the serial-port-adapter to
                    // occur.
                    this.autoResetEvent.Set();
                    return true;
                }

                // Trigger the cancellation of the receive routine.
                this.cancellationTokenSource.Cancel();

                return true;
            }
            finally
            {
                // Disconnect the event handlers first.
                this.commsAdapter.DataReceived -= CommsAdapter_DataReceived;
                if (this.commsAdapter is ISerialPortAdapter serialPortAdapter)
                    serialPortAdapter.SerialPortError -= SerialPortAdapter_SerialPortError;

                // Wait for up to one second for the receive processing to finish;
                this.autoResetEvent.WaitOne(1000);
                // then close the serial port.
                this.commsAdapter.Disconnect();

                logger.Debug(() => "Disconnected");
            }
        }

        #endregion

        #region Events

        public event EventHandler<GridConnectMessageEventArgs> MessageReceived;

        #endregion

        #region Support routines

        /// <summary>
        /// The main processing routine for received data.
        /// </summary>
        /// <remarks>
        /// Data will be received in the correct sequence, but may be split into arbitary blocks.
        /// This means that received data must be concatenated to allow individual messages to be
        /// extracted in the context of any adjacent messages. The necesitates nested loops.
        /// If no data is received for a while this routine must wait until it is prompted to do
        /// further processing: a ManualResetEvent handles this. Within that it must loop to
        /// remove data from the queue. Then inside that it must loop extract each individual 
        /// message from the current data. All the time monitoring the cancellationToken in case
        /// the connection is closed.
        /// </remarks>
        private void ProcessSerial()
        {
            logger.Trace(() => nameof(ProcessSerial));

            // Signal that the processing routine has started.
            this.autoResetEvent.Reset();

            try
            {
                // Any characters left over from the previous attemp to process a message.
                var previousText = string.Empty;

                // Keep running until the process is cancelled.
                while (!this.cancellationTokenSource.IsCancellationRequested)
                {
                    // Ensure that the ResetEvent is not flagged; then wait for it to be flagged.
                    this.manualResetEvent.Reset();
                    this.manualResetEvent.Wait(this.cancellationTokenSource.Token);

                    // Keep dequeueing received text until we either run out or the process is
                    // cancelled.
                    while (!this.cancellationTokenSource.IsCancellationRequested && 
                            this.receiveQueue.TryDequeue(out var text))
                    {
                        // Append the text we've just retrieved to any that was left over from last
                        // time.
                        previousText += text;

                        // Keep processing messages out of the text until there are none left, or
                        // the process is cancelled.
                        do
                        {
                            logger.Trace(() => previousText);
                        }
                        while (!this.cancellationTokenSource.IsCancellationRequested && 
                                ProcessText(ref previousText));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // If the manual reset event is waiting when the cancellation token is cancelled it
                // it will throw this exception; just log it and continue.
                logger.Trace(() => $"{nameof(ProcessSerial)} cancelled.");
            }
            catch(Exception ex)
            {
                logger.Info(() => $"{nameof(ProcessSerial)} errored.");
                logger.Error(ex);
            }
            finally
            {
                // Clear down the receive queue to prevent old messages being processed if we are
                // connected subsequently.
                this.receiveQueue.Clear();

                logger.Trace(() => $"{nameof(ProcessSerial)} terminated.");

                // Signal that the processing routine has terminated.
                this.autoResetEvent.Set();
            }
        }

        /// <summary>
        /// Process the specified <paramref name="text"/> and extract the first GridConnect
        /// message from it. Return the remaining text. If there is an incomplete message at the 
        /// start then discard it and extract the following message.
        /// </summary>
        /// <param name="text">A string containing the text to process.</param>
        /// <returns>True if further processing may be necessary; false otherwise.</returns>
        private bool ProcessText(ref string text)
        {
            logger.Trace(() => nameof(ProcessText));

            // If there is no text then we cannot process, so processing has finished.
            if (string.IsNullOrEmpty(text)) return false;

            // If the text doesn't start with the expected initial character ':' then we have the
            // end of an incomplete message; the only thing we can do is throw away those characters.
            if (text[0] != MESSAGE_START)
            {
                // If there is another message in the text then strip off the preceeding characters
                // and process that; if there isn't then clear the text and signal that processing
                // has finished.
                var nextPosition = text.IndexOf(MESSAGE_START);
                if (nextPosition == -1)
                {
                    text = string.Empty;
                    return false;
                }
                    
                text = text[nextPosition..];
            }

            // Find the position of the message terminating character; if there isn't one then the
            // text doesn't contain a complete message, so signal that processing has finished.
            var position = text.IndexOf(MESSAGE_TERMINATE);
            if (position == -1) return false;

            // Extract first entire message and handle it; then remove it from the text that
            // remains to be processed and signal that processing is to continue.
            OnMessageReceived(text[..position]);
            text = text[(position + 1)..];

            return true;
        }

        /// <summary>
        /// Raise an event for the specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string containing the message.</param>
        protected virtual void OnMessageReceived(string message)
        {
            var payload = HexStringToByteArray(message[7..].TrimEnd(MESSAGE_TERMINATE));

            this.MessageReceived?.Invoke(this,
                new GridConnectMessageEventArgs
                {
                    Message = message,
                    Payload = payload,
                });
        }

        /// <summary>
        /// Convert the specified <paramref name="hex"/> to a <see cref="byte[]"/> .
        /// </summary>
        /// <param name="hex">A string containing the hex.</param>
        /// <returns>A <see cref="byte[]"/>.</returns>
        /// <remarks>
        /// The string contains pairs of characters that form bytes. Iterate through those pairs,
        /// convert them to bytes, and return them as an array.
        /// </remarks>
        private static byte[] HexStringToByteArray(string hex)
        {
            var length = hex.Length;
            var data = new byte[length / 2];
            for (var i = 0; i < length; i += 2)
                if (byte.TryParse(hex[i..(i + 2)], NumberStyles.HexNumber, null, out var value))
                    data[i / 2] = value;

            return data;
        }

        #endregion

        #region Event handler routines

        /// <summary>
        /// Event handler routine for the <see cref="SerialPortAdapter"/> <see cref="SerialPortAdapter.SerialPortError"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The <see cref="SerialPortErrorEventArgs"/> for the event.</param>
        private void SerialPortAdapter_SerialPortError(object sender, SerialPortErrorEventArgs e)
        {
            // Log the details of a serial port error.

            logger.Trace(() => nameof(SerialPortAdapter_SerialPortError));
            logger.Error(e.Exception);
        }

        /// <summary>
        /// Event handler routine for the <see cref="ICommsAdapter"/> <see cref="ICommsAdapter.DataReceived"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The <see cref="DataReceivedEventArgs"/> for the event.</param>
        private void CommsAdapter_DataReceived(object sender, DataReceivedEventArgs e)
        {
            logger.Trace(() => nameof(CommsAdapter_DataReceived));

            // Convert the received bytes into chars, concatenate them into a string and put it in
            // the received message queue. Then flag to the ProcessQueue routine that is has work
            // to do.
            this.receiveQueue.Enqueue(new(e.Data.Select(b => (char)b).ToArray()));
            this.manualResetEvent.Set();
        }

        #endregion
    }
}
