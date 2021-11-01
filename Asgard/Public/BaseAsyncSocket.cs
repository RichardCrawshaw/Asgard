using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;

namespace Asgard
{
    public abstract class BaseAsyncSocket :
        IDisposable
    {
        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly CancellationTokenSource cancellationTokenSource = new();

        // Backing fields are used for ip-address and port to prevent them from being changed when
        // the current instance is connected.
        private IPAddress ipAddress = null;
        private int port;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets and sets the address as a <see cref="string"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Address"/> cannot be changed while <see cref="IsConnected"/> is true.
        /// </remarks>
        public string Address
        {
            get => this.ipAddress.ToString();
            set
            {
                if (!this.IsConnected)
                    this.ipAddress = IPAddress.Parse(value);
            }
        }

        /// <summary>
        /// Get the handle of the underlying <see cref="Socket"/>.
        /// </summary>
        public abstract long? Handle { get; }

        /// <summary>
        /// Gets and sets the address as an <see cref="IPAddress"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="IPAddress"/> cannot be changed while <see cref="IsConnected"/> is true.
        /// </remarks>
        public IPAddress IPAddress
        {
            get => this.ipAddress;
            set
            {
                if (!this.IsConnected)
                    this.ipAddress = value;
            }
        }

        /// <summary>
        /// Gets whether the current instance is connected.
        /// </summary>
        public abstract bool IsConnected { get; }

        /// <summary>
        /// Gets whether this isntance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets and sets the port.
        /// </summary>
        /// <remarks>
        /// The <see cref="Port"/> cannot be changed while <see cref="IsConnected"/> is true.
        /// </remarks>
        public int Port
        {
            get => this.port;
            set
            {
                if (!this.IsConnected)
                    this.port = value;
            }
        }

        #endregion

        #region Protected properties

        /// <summary>
        /// Gets and sets the encoding used when converting between byte data and text.
        /// </summary>
        protected Encoding Encoding { get; set; } = Encoding.ASCII;

        /// <summary>
        /// Gets the <see cref="CancellationToken"/> used to signal the cancellation of waiting 
        /// processes.
        /// </summary>
        protected CancellationToken Token => this.cancellationTokenSource.Token;

        /// <summary>
        /// Gets whether cancellation has been requested.
        /// </summary>
        protected bool IsCancellationRequested => this.cancellationTokenSource.IsCancellationRequested;

        #endregion

        #region Constructors

        protected BaseAsyncSocket() { }

        #endregion

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.IsDisposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BaseAsyncSocket()
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

        #region Abstract methods

        /// <summary>
        /// Send the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="data">A <see cref="byte[]"/> containing the data to send.</param>
        /// <returns></returns>
        public abstract bool Send(byte[] data);

        /// <summary>
        /// Send the specified <paramref name="text"/>.
        /// </summary>
        /// <param name="text">A <see cref="string"/> containing the text to send.</param>
        public abstract bool Send(string text);

        #endregion

        #region Events

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public event EventHandler<SocketClosedEventArgs> SocketClosed;


        //public event EventHandler<SocketMessageReceivedEventArgs> MessageReceived;

        #endregion

        #region Virtual support routines

        /// <summary>
        /// Override this routine with <see cref="Send(Socket, byte[])"/> to echo the received 
        /// <paramref name="data"/> to the <paramref name="socket"/> of the sending client.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to echo the <paramref name="data"/> to.</param>
        /// <param name="data">A <see cref="byte[]"/> containint the data to echo.</param>
        protected virtual void Echo(Socket socket, byte[] data) { }

        #endregion

        #region Protected support routines

        /// <summary>
        /// Cancel all waiting threads ready for closing down.
        /// </summary>
        protected void Cancel() => this.cancellationTokenSource.Cancel();

        protected static void CloseSocket(Socket socket)
        {
            if (socket is null) return;

            socket.Close();
            logger.Trace(() => $"Socket closed: {socket.Handle}.");
        }

        /// <summary>
        /// Get the <see cref="IPEndPoint"/> of the current instance.
        /// </summary>
        /// <returns></returns>
        protected IPEndPoint GetEndPoint() => new(this.IPAddress, this.Port);

        /// <summary>
        /// Get the <see cref="Socket"/> of the current instance.
        /// </summary>
        /// <returns></returns>
        protected Socket GetSocket() => new(this.IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// Read incoming data from the specified <paramref name="socket"/>.
        /// </summary>
        /// <param name="socket"></param>
        protected void Read(Socket socket)
        {
            logger.Trace(() => nameof(Read));
            logger.Trace(() => $"{nameof(socket)}: {socket.Handle} connected is {socket.Connected}.");

            if (!socket.Connected) return;

            try
            {
                // Create the state object.  
                var state = new StateObject
                {
                    Socket = socket,
                };

                // Begin receiving the data from the remote device.  
                socket.BeginReceive(state.Buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        /// <summary>
        /// Send the specified <paramref name="text"/> via the specified <paramref name="socket"/>.
        /// </summary>
        /// <param name="socket">A <see cref="Socket"/> instance.</param>
        /// <param name="text">A <see cref="string"/> containing the text to send.</param>
        protected void Send(Socket socket, string text)
        {
            logger.Trace(() => nameof(Send));

            // Convert the string data to byte data using the defined encoding.  
            var data = this.Encoding.GetBytes(text);

            // Begin sending the data to the remote device.  
            Send(socket, data);
        }

        /// <summary>
        /// Send the specified <paramref name="data"/> via the specified <paramref name="socket"/>.
        /// </summary>
        /// <param name="socket">A <see cref="Socket"/> instance.</param>
        /// <param name="data">A <see cref="byte[]"/> containing the data to send.</param>
        protected void Send(Socket socket, byte[] data)
        {
            logger.Trace(() => nameof(Send));

            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
        }

        protected static void ShutdownSocket(Socket socket)
        {
            if (socket is null) return;

            if (socket.Connected)
                socket.Shutdown(SocketShutdown.Both);

            logger.Trace(() => $"Socket shutdown: {socket.Handle}.");
        }

        #endregion

        #region Private support routines

        /// <summary>
        /// Call this method to trigger the closing of the specified <paramref name="socket"/>.
        /// </summary>
        /// <param name="socket"></param>
        private void OnClosed(Socket socket)
        {
            if (socket is not null)
                this.SocketClosed?.Invoke(this,
                    new SocketClosedEventArgs(socket));
            logger.Trace(() => $"Connection closed: {socket?.Handle}.");
        }

        /// <summary>
        /// Call this method to handle the receipt of <paramref name="data"/>.
        /// </summary>
        /// <param name="data">A <see cref="byte[]"/> containing the received data.</param>
        private void OnDataReceived(byte[] data)
        {
            this.DataReceived?.Invoke(this,
                new DataReceivedEventArgs(data));
        }

        /// <summary>
        /// A callback routine that handles incoming messages using the specified <paramref name="asyncResult"/>.
        /// Once a message has been received reading is initiated again.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> object.</param>
        /// <remarks>
        /// If the <see cref="Socket"/> is either not <see cref="Socket.Connected"/>, or zero bytes
        /// of data is received, or an <see cref="Exception"/> occurs then the connection is closed.
        /// </remarks>
        private void ReadCallback(IAsyncResult asyncResult)
        {
            logger.Trace(() => nameof(ReadCallback));

            // Retrieve the state object and the client socket
            // from the asynchronous state object.  
            if (asyncResult.AsyncState is not StateObject client)
            {
                logger.Error(() => "Reading failed.");
                logger.Warn(() => $"Failed to get {nameof(client)} from {nameof(asyncResult)} when reading.");
                return;
            }

            var close = true;
            if (!client.Socket.Connected)
                logger.Trace(() => $"Socket is disconnected: {client.Socket.Handle}.");
            else if (ReadCallback(asyncResult, client, client.Socket))
                close = false;

            if (close)
                OnClosed(client.Socket);
        }

        private bool ReadCallback(IAsyncResult asyncResult, StateObject client, Socket socket)
        {
            try
            {
                // Read data from the remote device.  
                var count = socket.EndReceive(asyncResult);
                if (count == 0)
                {
                    logger.Trace(() => $"No data received from socket: {socket.Handle}. Closing connection.");
                    return false;
                }
                logger.Debug(() => $"Read {count} bytes from socket: {socket.Handle}.");

                // Handle the received data.
                OnDataReceived(client.Buffer[0..count]);

                // Echo the received data back to the originating client.
                Echo(socket, client.Buffer[0..count]);

                // Initiate reading the next data.
                Read(socket);

                return true;
            }
            catch (SocketException ex)
            {
                logger.Info($"Socket error from {socket?.Handle}: {ex.SocketErrorCode}.");
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                logger.Warn(() => $"Reading from {socket?.Handle} ended unexpectedly.");
                return false;
            }
        }

        /// <summary>
        /// A callback routine that processes the specified <paramref name="asyncResult"/>.
        /// </summary>
        /// <param name="asyncResult">The <see cref="IAsyncResult"/> instance to process.</param>
        private void SendCallback(IAsyncResult asyncResult)
        {
            logger.Trace(() => nameof(SendCallback));

            try
            {
                // Retrieve the socket from the state object.  
                if (asyncResult.AsyncState is not Socket socket)
                {
                    logger.Error(() => "Sending failed.");
                    logger.Warn(() => $"Failed to get {nameof(socket)} from {nameof(asyncResult)} when sending.");
                    return;
                }

                // Complete sending the data to the remote device.  
                var count = socket.EndSend(asyncResult);
                logger.Debug(() => $"Sent {count} bytes to socket {socket.Handle}.");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        #endregion

        /// <summary>
        /// State object for reading client data asynchronously
        /// </summary>  
        private class StateObject
        {
            #region Fields

            // Size of receive buffer.  
            public const int BUFFER_SIZE = 1024;

            #endregion

            #region Properties

            /// <summary>
            /// Receive buffer.
            /// </summary>
            public byte[] Buffer { get; } = new byte[BUFFER_SIZE];

            /// <summary>
            /// Client socket.
            /// </summary>
            public Socket Socket { get; set; } = null;

            #endregion
        }
    }
}
