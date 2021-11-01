using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;

namespace Asgard
{
    /// <summary>
    /// Class to provide <see cref="Socket"/> server capability.
    /// </summary>
    public class AsyncSocketServer : BaseAsyncSocket,
        ISocketServerAdapter
    {
        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Thread signals to synchronise between different threads.
        private readonly ManualResetEventSlim listenerStarted = new(false);
        private readonly ManualResetEventSlim listenerConnected = new(false);

        /// <summary>
        /// Holds a list of <see cref="Socket"/> objects for the connected clients.
        /// </summary>
        private readonly List<Socket> sockets = new();

        /// <summary>
        /// Locking object to manage access to the <see cref="sockets"/>.
        /// </summary>
        private readonly object socketsLock = new();

        /// <summary>
        /// The <see cref="ISettings"/> object.
        /// </summary>
        private readonly ISettings settings;

        /// <summary>
        /// Backing field for <see cref="IsConnected"/>.
        /// </summary>
        /// <remarks>
        /// The overridden abstract property cannot be extended to support a private (or protected) 
        /// setter. The abstract property cannot be defined with a protected setter as not all
        /// derived classes should be able to change the state independently.
        /// </remarks>
        private bool isConnected;

        private Socket socket = null;

        #endregion

        #region Properties

        /// <summary>
        /// Get the handle of the underlying <see cref="Socket"/>.
        /// </summary>
        public override long? Handle => this.socket?.Handle.ToInt64();

        /// <summary>
        /// Gets whether the current instance is connected.
        /// </summary>
        public override bool IsConnected => this.isConnected;

        #endregion

        #region Constructors

        public AsyncSocketServer(ISettings settings)
            : base()
        {
            logger.Trace(() => nameof(AsyncSocketServer));

            this.settings = settings;

            var settingsNode = this.settings.Get<AsyncSocketServer, ServerSettings>();

            if (string.IsNullOrEmpty(settingsNode.Address) ||
                settingsNode.Address == ".")
            {
                var hostName = Dns.GetHostName();
                var ipHostInfo = Dns.GetHostEntry(hostName);
                base.IPAddress = ipHostInfo.AddressList[0];
            }
            else
                base.Address = settingsNode.Address;
            base.Port = settingsNode.Port;

            this.SocketClosed += AsyncSocketListener_SocketClosed;
        }

        #endregion

        #region IDisposable support

        protected override void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    CloseSocket(this.socket);
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disconnect from the specified <see cref="Socket"/>.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> that is to be disconnected.</param>
        public void Disconnect(Socket socket)
        {
            logger.Trace(() => nameof(Disconnect));

            if (SocketConnected(socket))
            {
                Send(socket, string.Empty);
                SocketRemove(socket);
            }
        }

        /// <summary>
        /// Send the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="data">A <see cref="byte[]"/> containing the data to send.</param>
        /// <returns></returns>
        public override bool Send(byte[] data)
        {
            logger.Trace(() => nameof(Send));

            if (!this.isConnected) return false;

            var sockets = SocketsGet();
            foreach (var socket in sockets)
                if (socket.Connected)
                    Send(socket, data);

            return true;
        }

        /// <summary>
        /// Send the specified <paramref name="text"/>.
        /// </summary>
        /// <param name="text">A <see cref="string"/> containing the text to send.</param>
        public override bool Send(string text)
        {
            logger.Trace(() => nameof(Send));

            if (!this.IsConnected) return false;

            var sockets = SocketsGet();
            foreach (var socket in sockets)
                if (socket.Connected)
                    Send(socket, text);

            return true;
        }

        /// <summary>
        /// Initiate listening for a connection.
        /// </summary>
        public void Start()
        {
            logger.Trace(() => nameof(Start));

            // Establish the local endpoint for the socket from the IP Address and Port.
            var localEndPoint = GetEndPoint();

            // Close the socket prior to getting a new one.
            CloseSocket(this.socket);

            // Create a TCP/IP socket.  
            this.socket = GetSocket();

            // Bind the socket to the local endpoint and listen for incoming connections.  
            this.socket.Bind(localEndPoint);
            this.socket.Listen(100);

            // Start the listener process.
            ThreadPool.QueueUserWorkItem(_ => Connect(this.socket));

            try
            {
                // Wait for the listener process to start.
                this.listenerStarted.Wait(this.Token);
                logger.Trace(() => "Server started.");

                // Wait for a connection to come in.
                this.listenerConnected.Wait(this.Token);
                logger.Trace(() => "Listener connected.");
                this.isConnected = true;
            }
            catch (OperationCanceledException)
            {
                logger.Trace(() => $"Listening cancelled (in {nameof(Start)}).");
            }
        }

        /// <summary>
        /// Stop listening for new connections and disconnect from all existing connections.
        /// </summary>
        public void Stop()
        {
            logger.Trace(() => nameof(Stop));

            Cancel();

            ShutdownSocket(this.socket);
        }

        #endregion

        #region Override support routines

        /// <summary>
        /// Handle the echoing of received data to the sending client.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> that the <paramref name="data"/> is to be echoed to.</param>
        /// <param name="data">A <see cref="byte[]"/> containing the data to echo.</param>
        protected override void Echo(Socket socket, byte[] data)
        {
            logger.Trace(() => nameof(Echo));

            Send(socket, data);
        }

        #endregion

        #region Support routines

        /// <summary>
        /// Start the connection process using the specified <paramref name="listener"/>.
        /// </summary>
        /// <param name="listener">A <see cref="Socket"/> to listen for new connections.</param>
        /// <remarks>
        /// Needs to be run on a separate thread.
        /// </remarks>
        private void Connect(Socket listener)
        {
            logger.Trace(() => nameof(Connect));
            logger.Trace(() => $"{nameof(listener)}: {listener.Handle} listening for connections {listener.Connected}.");

            try
            {
                while (!this.IsCancellationRequested)
                {
                    // Flag that the listening loop has started.
                    this.listenerStarted.Set();
                    logger.Trace(() => "ListenerStarted: Set");

                    // Set the event to nonsignaled state, just to be sure.
                    this.listenerConnected.Reset();
                    logger.Trace(() => "ListenerConnected: Reset");

                    // Start an asynchronous socket to listen for connections.  
                    logger.Trace(() => "Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(ConnectCallback),
                        listener);

                    // Wait until a connection is made before continuing.
                    logger.Trace(() => "ListenerConnected: Wait");
                    this.listenerConnected.Wait(this.Token);
                }
            }
            catch (OperationCanceledException)
            {
                logger.Trace(() => $"Listening cancelled for {listener.Handle}.");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                logger.Trace(() => $"Finished listening on {listener.Handle}.");
            }
        }

        /// <summary>
        /// A callback routine that handles incoming connections using the specified <paramref name="asyncResult"/>.
        /// </summary>
        /// <param name="asyncResult">The <see cref="IAsyncResult"/> instance to process.</param>
        private void ConnectCallback(IAsyncResult asyncResult)
        {
            logger.Trace(() => nameof(ConnectCallback));

            try
            {
                // Flag that a client has connected.
                this.listenerConnected.Set();
                logger.Trace(() => "ListenerConnected: Set");

                // Get the socket that handles the client request.  
                if (asyncResult.AsyncState is not Socket socket)
                {
                    logger.Error(() => "Connection failed.");
                    logger.Warn(() => $"Failed to get {nameof(socket)} from {nameof(asyncResult)} when connecting.");
                    return;
                }

                // If cancellation is happening then we must exit here before attempting to
                // access the socket, as it has probably already been disposed.
                if (this.IsCancellationRequested)
                {
                    logger.Trace(() => $"Listening cancelled on {socket.Handle}.");
                    return;
                }

                // Accept the connection.
                var handler = socket.EndAccept(asyncResult);
                SocketAdd(handler);

                logger.Trace(() => $"Connection recd: {this.isConnected} ({socket.Connected}) {socket.Handle}.");
                logger.Trace(() => $"Connection made: {this.IsConnected} ({handler.Connected}) {handler.Handle}.");

                // Initiate receiving data...
                Read(handler);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                logger.Warn(() => $"Connection to {(asyncResult?.AsyncState as Socket)?.Handle} ended.");
            }
        }

        /// <summary>
        /// Add the specified <paramref name="socket"/> to the connected <see cref="Socket"/> objects.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to add.</param>
        private void SocketAdd(Socket socket)
        {
            lock (this.socketsLock)
                this.sockets.Add(socket);
        }

        /// <summary>
        /// Determines if the specified <paramref name="socket"/> is connected to this instance.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to check.</param>
        /// <returns>True if it is connected to this instance; false otherwise.</returns>
        private bool SocketConnected(Socket socket)
        {
            lock (this.socketsLock)
                return this.sockets.Contains(socket);
        }

        /// <summary>
        /// Gets all the <see cref="Socket"/> objects connected to this instance.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of <see cref="Socket"/> objects.</returns>
        private List<Socket> SocketsGet()
        {
            var results = new List<Socket>();
            lock (this.socketsLock)
                results.AddRange(this.sockets);
            return results;
        }

        /// <summary>
        /// Removes the specified <paramref name="socket"/> from the connected <see cref="Socket"/> objects.
        /// </summary>
        /// <param name="socket"></param>
        private void SocketRemove(Socket socket)
        {
            lock (this.socketsLock)
                this.sockets.Remove(socket);
        }

        #endregion

        #region Event handler routines

        /// <summary>
        /// The event handler routine for when a connected <see cref="Socket"/> is closed.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The <see cref="SocketClosedEventArgs"/> data.</param>
        private void AsyncSocketListener_SocketClosed(object sender, SocketClosedEventArgs e)
        {
            if (e?.Socket is not null)
                SocketRemove(e.Socket);
        }

        #endregion
    }
}
