using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;

namespace Asgard
{
    class SocketServerProcessor :
        ISocketServerAdapter
    {
        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly CancellationTokenSource cancellationTokenSource = new();

        private readonly ManualResetEvent manualResetEvent = new(false);

        /// <summary>
        /// A list of the clients that have connected to this instance.
        /// </summary>
        private readonly List<Client> clients = new();

        /// <summary>
        /// Indicates that this instance has been instructed to allow incoming connections.
        /// </summary>
        private bool isConnecting = false;

        private readonly int backlogCount = 100;

        #endregion

        #region Properties

        public bool IsConnected { get; private set; }
        public bool IsDisposed { get; private set; }
        public int Port { get; set; }

        private CancellationToken Token => this.cancellationTokenSource.Token;

        #endregion

        #region Constructors

        public SocketServerProcessor()
        {
            logger.Trace(() => nameof(SocketServerProcessor));
        }

        #endregion

        #region IDisposable support

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
        // ~SocketProcessor()
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
        /// Allow clients to connect to the current instance.
        /// </summary>
        public void Start()
        {
            logger.Trace(() => nameof(Start));

            if (this.IsConnected) return;
            this.isConnecting = true;

            ThreadPool.QueueUserWorkItem(_ => Listen());
        }

        /// <summary>
        /// Stop clients from attempting to connect to the current instance.
        /// </summary>
        public void Stop()
        {
            logger.Trace(() => nameof(Disconnect));

            this.cancellationTokenSource.Cancel();

            foreach (var client in this.clients)
                client.Socket.Shutdown(SocketShutdown.Both);
            foreach (var state in this.clients)
                state.Socket.Close();
        }

        /// <summary>
        /// Disconnect the specified <paramref name="socket"/> from the current instance.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> that is to be disconnected.</param>
        public void Disconnect(Socket socket)
        {
            logger.Trace(() => nameof(Disconnect));

            var clients =
                this.clients
                    .Where(n => n.Socket == socket)
                    .ToList();
            if (!clients.Any()) return;

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            this.clients
                .RemoveAll(c => c.Socket == socket);

            logger.Debug(() => $"{nameof(Socket)} {socket.LocalEndPoint} has been disconnected.");
        }

        /// <summary>
        /// Send the specified <paramref name="text"/> to all connected clients.
        /// </summary>
        /// <param name="text">A <see cref="string"/> containing the text that is to be sent.</param>
        public void Send(string text)
        {
            logger.Trace(() => nameof(Send));

            var data = Encoding.ASCII.GetBytes(text);
            Send(data);
        }

        /// <summary>
        /// Send the specified <paramref name="data"/> to all connected clients.
        /// </summary>
        /// <param name="data">A <see cref="byte[]"/> containing the data that is to be sent.</param>
        public void Send(byte[] data)
        {
            logger.Trace(() => nameof(Send));

            foreach(var state in this.clients)
                state.Socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(CallbackSend), state.Socket);
        }

        #endregion

        #region Events

        public event EventHandler<SocketMessageReceivedEventArgs> MessageReceived;

        #endregion

        #region Support routines

        /// <summary>
        /// The callback routine for the <see cref="Listen"/> routine using the specified 
        /// <paramref name="asyncResult"/>.
        /// </summary>
        /// <param name="asyncResult">The <see cref="IAsyncResult"/> that represents the result of the <see cref="Listen"/>.</param>
        private void CallbackListen(IAsyncResult asyncResult)
        {
            logger.Trace(() => nameof(CallbackListen));

            // Signal the listen thread to continue.
            this.manualResetEvent.Set();

            try
            {
                // Get the socket that handles the client request.
                if (asyncResult?.AsyncState is not Socket socket) return;

                // Create the object that represents the client that has connected.
                var client = new Client(socket.EndAccept(asyncResult));
                this.clients.Add(client);

                // The initial connecting stage has completed.
                this.IsConnected = true;
                this.isConnecting = false;

                // Initiate the receiving process.
                Receive(client);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        /// <summary>
        /// The callback routine for the <see cref="Receive(Client)"/> routine using the specified
        /// <paramref name="asyncResult"/>.
        /// </summary>
        /// <param name="asyncResult">The <see cref="IAsyncResult"/> that represents the result of the <see cref="Receive(Client)"/>.</param>
        private void CallbackReceive(IAsyncResult asyncResult)
        {
            logger.Trace(() => nameof(CallbackReceive));

            try
            {
                // Retrive the client from the async result.
                if (asyncResult.AsyncState is not Client client) return;

                // Read data from the connected socket.
                var count = client.Socket.EndReceive(asyncResult);

                // Make sure that we're still wanting to handle received data and data received from
                // this client.
                if (this.Token.IsCancellationRequested) return;
                if (!this.clients.Contains(client)) return;

                if (count > 0)
                {
                    // Forward the received data to any subscribed observers.
                    ThreadPool.QueueUserWorkItem(_ =>
                        this.MessageReceived?.Invoke(this,
                            new SocketMessageReceivedEventArgs(client.Buffer)));
                }

                // Receive more data.
                Receive(client);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        /// <summary>
        /// The callback routine for the <see cref="Send(byte[])"/> routine using the specified
        /// <paramref name="asyncResult"/>.
        /// </summary>
        /// <param name="asyncResult">The <see cref="IAsyncResult"/> that represents the result of the <see cref="Send(byte[])"/>.</param>
        private void CallbackSend(IAsyncResult asyncResult)
        {
            logger.Trace(() => nameof(CallbackSend));

            try
            {
                // Retrieve the socket from the state object.
                if (asyncResult.AsyncState is not Socket handler) return;

                // Complete sending the data to the remote end-point.
                var count = handler.EndSend(asyncResult);

                logger.Debug(() => $"{count} bytes sent to remote end-point.");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        /// <summary>
        /// Listen for incoming connections and allow them to connect.
        /// </summary>
        /// <remarks>
        /// Should be run on a separate thread.
        /// </remarks>
        private void Listen()
        {
            logger.Trace(() => nameof(Listen));

            if (this.IsConnected || !this.isConnecting) return;

            // Get our local end-point
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddress, this.Port);

            // Create a socket to listen.
            var listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                // Bind to the local endpoint and listen for new connections with the defined
                // backlog size.
                listener.Bind(localEndPoint);
                listener.Listen(this.backlogCount);

                // Keep checking for incoming connections until cancelled.
                while (!this.Token.IsCancellationRequested)
                {
                    this.manualResetEvent.Reset();

                    logger.Debug(() => "Waiting for connection.");

                    listener.BeginAccept(
                        new AsyncCallback(CallbackListen),
                        listener);

                    // Wait for a connection to be made.
                    this.manualResetEvent.WaitOne();
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                logger.Trace(() => $"{nameof(Listen)} finished.");
            }
        }

        /// <summary>
        /// Receive data from the specified <paramref name="client"/>.
        /// </summary>
        /// <param name="client">The <see cref="Client"/> from which to receive data.</param>
        private void Receive(Client client)
        {
            logger.Trace(() => nameof(Receive));

            client.Socket.BeginReceive(client.Buffer, 0, Client.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(CallbackReceive), client);
        }

        #endregion

        /// <summary>
        /// Object for managing the reading of client data asynchronously
        /// </summary>  
        protected class Client
        {
            #region Fields

            public const int BUFFER_SIZE = 1024;

            #endregion

            #region Properties

            public byte[] Buffer { get; } = new byte[BUFFER_SIZE];

            public Socket Socket { get; set; } = null;

            #endregion

            #region Constructors

            public Client(Socket socket)
            {
                this.Socket = socket;
            }

            #endregion
        }
    }
}
