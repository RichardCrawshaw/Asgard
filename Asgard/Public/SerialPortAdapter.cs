using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using NLog;

namespace Asgard
{
    /// <summary>
    /// Adapter class for <see cref="SerialPort"/> that fixes a number of its issues. See this blog
    /// post by Ben Voigt:
    /// https://www.sparxeng.com/blog/software/must-use-net-system-io-ports-serialport
    /// </summary>
    /// <remarks>
    /// Additional error handling added to cope with the COM port being unplugged while in use, 
    /// including automatic re-connection.
    /// </remarks>
    public class SerialPortAdapter :
        ISerialPortAdapter
    {
        #region Members

        /// <summary>
        /// The default read buffer size.
        /// </summary>
        const int READ_BUFFER_SIZE = 64;

        /// <summary>
        /// Internal open flag.
        /// </summary>
        private bool isOpen = false;

        /// <summary>
        /// Logger.
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The underlying serial port.
        /// </summary>
        private readonly SerialPort serialPort;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the <see cref="SerialPortAdapter"/> has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets and sets the baud rate.
        /// </summary>
        public int BaudRate
        {
            get => this.serialPort.BaudRate;
            set
            {
                if (!this.IsConnected)
                    this.serialPort.BaudRate = value;
            }
        }

        /// <summary>
        /// Gets and sets the read buffer size.
        /// </summary>
        public uint BufferSize { get; set; } = READ_BUFFER_SIZE;

        /// <summary>
        /// Gets and sets the data bits.
        /// </summary>
        public int DataBits
        {
            get => this.serialPort.DataBits;
            set
            {
                if (!this.IsConnected)
                    this.serialPort.DataBits = value;
            }
        }

        /// <summary>
        /// Gets whether the underlying serial port is open.
        /// </summary>
        /// <remarks>
        /// Also needs to consider the internal is-open flag, as this represents whether the port
        /// is EXPECTED to be open.
        /// </remarks>
        public bool IsConnected => this.serialPort.IsOpen && this.isOpen;

        /// <summary>
        /// Gets the name of the COM port that the underlying serial port is connected to.
        /// </summary>
        public string Name => this.serialPort?.PortName ?? "Unknown";

        /// <summary>
        /// Gets and sets the parity.
        /// </summary>
        public Parity Parity
        {
            get => this.serialPort.Parity;
            set
            {
                if (!this.IsConnected)
                    this.serialPort.Parity = value;
            }
        }

        /// <summary>
        /// Gets the name of the port.
        /// </summary>
        public string PortName
        {
            get => this.serialPort.PortName;
            set
            {
                if (!this.IsConnected)
                    this.serialPort.PortName = value;
            }
        }

        /// <summary>
        /// Gets and sets the stop bits.
        /// </summary>
        public StopBits StopBits
        {
            get => this.serialPort.StopBits;
            set
            {
                if (!this.IsConnected)
                    this.serialPort.StopBits = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Stream"/> of the underlying serial port; or null if not available.
        /// </summary>
        public Stream Stream => this.serialPort?.BaseStream;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="SerialPortAdapter"/>.
        /// </summary>
        public SerialPortAdapter()
        {
            logger.Trace(() => nameof(SerialPortAdapter));

            this.serialPort = new();
        }

        #endregion

        #region IDisposable support

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    if (this.serialPort != null)
                    {
                        if (this.serialPort.IsOpen)
                            this.serialPort.Close();
                        this.serialPort.Dispose();
                    }
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                this.IsDisposed = true;
            }
        }

        // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SerialPortAdapter()
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
        /// Close the underlying serial port.
        /// </summary>
        public void Disconnect()
        {
            logger.Trace(() => nameof(Disconnect));

            this.isOpen = false;
            this.serialPort.Close();

            logger.Debug(() => "Closed.");
        }

        /// <summary>
        /// Open the underlying serial port.
        /// </summary>
        public void Connect()
        {
            logger.Trace(() => nameof(Connect));
            logger.Debug(() => $"{this.PortName} {this.BaudRate} baud with {this.DataBits} data bits {this.StopBits} stop bits and {this.Parity} parity.");

            if (this.serialPort.IsOpen) return;

            this.serialPort.Open();

            logger.Info(() => $"Port opened: {this.serialPort.IsOpen}.");

            if (!this.serialPort.IsOpen) return;

            this.isOpen = true;

            ReadIncomingData(this.serialPort);
        }

        /// <summary>
        /// Sends the data in the specified <paramref name="buffer"/> via the underlying serial port.
        /// </summary>
        /// <param name="buffer">A buffer containing data to send.</param>
        public void Send(byte[] buffer) => this.serialPort.Write(buffer, 0, buffer.Length);

        /// <summary>
        /// Writes the specified <paramref name="text"/> to the underlying serial port.
        /// </summary>
        /// <param name="text">A string containing the text to send.</param>
        public void Send(string text) => this.serialPort.Write(text);

        #endregion

        #region Events

        /// <summary>
        /// Occurs when serial data is received.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>
        /// Occurs when an error occurs on the underlying serial port.
        /// </summary>
        public event EventHandler<SerialPortErrorEventArgs> SerialPortError;

        #endregion

        #region Support routines

        /// <summary>
        /// Handles the logging and onward transmission of the specified <paramref name="data"/> data.
        /// </summary>
        /// <param name="data">The data that has been received.</param>
        protected virtual void OnReceivedSerialData(byte[] data)
        {
            logger.Trace(() => nameof(OnReceivedSerialData));

            this.DataReceived?.Invoke(this.serialPort,
                new DataReceivedEventArgs(data));
        }

        /// <summary>
        /// Handles the onward transmission from the specified <paramref name="serialPort"/> of the
        /// specified <paramref name="ex"/>.
        /// </summary>
        /// <param name="serialPort">The <see cref="SerialPort"/>.</param>
        /// <param name="ex">The <see cref="Exception"/> that has occurred.</param>
        private void OnSerialPortError(SerialPort serialPort, Exception ex)
        {
            if (serialPort.IsOpen)
                OnSerialPortError(ex);
        }

        /// <summary>
        /// Handles the onward transmission of the specified <paramref name="ex"/>.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> that has occurred.</param>
        private void OnSerialPortError(Exception ex)
        {
            this.SerialPortError?.Invoke(this,
                new SerialPortErrorEventArgs
                {
                    Exception = ex,
                });
        }

        /// <summary>
        /// Handles the reading of incoming data on the specified <paramref name="serialPort"/>.
        /// </summary>
        /// <param name="serialPort">The serial port to monitor.</param>
        private void ReadIncomingData(SerialPort serialPort)
        {
            logger.Trace(() => nameof(ReadIncomingData));

            // Discard anything that had been received before the serial port was opened.
            serialPort.DiscardInBuffer();

            var buffer = new byte[this.BufferSize];
            void kickoffRead(Stream stream) => 
                stream.BeginRead(buffer, 0, buffer.Length,
                    delegate (IAsyncResult result)
                    {
                        try
                        {
                            if (serialPort.IsOpen)
                            {
                                var actualLength = stream.EndRead(result);
                                var received = new byte[actualLength];
                                Buffer.BlockCopy(buffer, 0, received, 0, actualLength);
                                OnReceivedSerialData(received);
                            }
                        }
                        catch (IOException ex)
                        {
                            OnSerialPortError(serialPort, ex);
                        }
                        catch (OperationCanceledException ex)
                        {
                            // This exception isn't documented, but Stream.EndRead can throw it if 
                            // the COM port is unplugged after Stream.BeginRead has been called.
                            OnSerialPortError(serialPort, ex);
                        }
                        catch (Exception ex)
                        {
                            OnSerialPortError(ex);
                        }

                        if (serialPort.IsOpen)
                            kickoffRead(stream);
                        else if (!this.IsDisposed && this.isOpen)
                        {
                            logger.Warn(() => $"{serialPort.PortName} has been closed.");

                            if (Reconnect(serialPort))
                            {
                                logger.Warn(() => $"{serialPort.PortName} has been re-opened.");
                                kickoffRead(serialPort.BaseStream);
                            }
                        }
                    },
                    null);

            kickoffRead(serialPort.BaseStream);

            logger.Trace(() => $"{this.serialPort.PortName} waiting for incoming data.");
        }

        /// <summary>
        /// Attempt to reconnect the specified <paramref name="serialPort"/>.
        /// </summary>
        /// <param name="serialPort">The <see cref="SerialPort"/> that is be reconnected.</param>
        /// <returns>True if the <paramref name="serialPort"/> was reconnected; false if it was explicitly closed, or the instance disposed.</returns>
        private bool Reconnect(SerialPort serialPort)
        {
            // Loop waiting for the serial port to be reconnected, or the instance to be disposed,
            // or the port to be closed (externally).
            while (!serialPort.IsOpen && !this.IsDisposed && this.isOpen)
            {
                Thread.Sleep(250);
                try
                {
                    serialPort.Open();
                }
                catch (FileNotFoundException) { }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Failed to reconnect {serialPort.PortName}.");
                    return false;
                }
            }

            // Make sure that the port is closed if the instance has been disposed, or it has been
            // explicitly closed.
            if (this.IsDisposed || !this.isOpen)
                if (serialPort.IsOpen)
                    serialPort.Close();

            return serialPort.IsOpen;
        }

        #endregion
    }
}
