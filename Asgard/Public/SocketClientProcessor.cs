using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Asgard
{
    internal class SocketClientProcessor :
        ISocketClientAdapter
    {
        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected int backlogCount = 100;

        protected string hostNameOrAddress = IPAddress.Loopback.ToString();

        private Socket socket = null;

        #endregion

        #region Properties

        public bool IsConnected => this.socket?.Connected ?? false;

        public bool IsDisposed { get; private set; }
        public int Port { get; set; }

        #endregion

        #region Constructors

        public SocketClientProcessor()
        {
            logger.Trace(() => nameof(SocketClientProcessor));
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
        // ~SocketClientProcessor()
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

        public void Connect()
        {
            logger.Trace(() => nameof(Connect));

            if (this.IsConnected) return;

            var ipHostInfo = Dns.GetHostEntry(this.hostNameOrAddress);
            var ipAddress = ipHostInfo.AddressList[0];
            var remoteEndPoint = new IPEndPoint(ipAddress, this.Port);

            this.socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            this.socket.BeginConnect(remoteEndPoint, new AsyncCallback(CallbackConnect), this.socket);
        }

        public void Disconnect()
        {
            logger.Trace(() => nameof(Disconnect));

            if (!this.IsConnected) return;

            this.socket?.Shutdown(SocketShutdown.Both);
            this.socket?.Close();

            logger.Debug(() => "Disconnected.");
        }

        public void Send(string text)
        {
            logger.Trace(() => nameof(Send));

            var data = Encoding.ASCII.GetBytes(text);

            Send(data);
        }

        public void Send(byte[] data)
        {
            logger.Trace(() => nameof(Send));

            this.socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(CallbackSend), this.socket);
        }

        #endregion

        #region Events

        public event EventHandler<SocketMessageReceivedEventArgs> MessageReceived;

        #endregion

        #region Support routines

        private void CallbackConnect(IAsyncResult asyncResult)
        {
            logger.Trace(() => nameof(CallbackConnect));

            try
            {
                if (asyncResult?.AsyncState is not Socket socket) return;

                socket.EndConnect(asyncResult);

                logger.Debug(() => $"Connected to {socket.RemoteEndPoint}.");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void CallbackReceive(IAsyncResult asyncResult)
        {
            logger.Trace(() => nameof(CallbackReceive));

            try
            {
                if (asyncResult.AsyncState is not ClientState client) return;

                var count = client.Socket.EndReceive(asyncResult);

                if (count > 0)
                    this.MessageReceived?.Invoke(this,
                        new SocketMessageReceivedEventArgs(client.Buffer));

                Receive();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void CallbackSend(IAsyncResult asyncResult)
        {
            try
            {
                if (asyncResult.AsyncState is not Socket socket) return;

                var count = socket.EndSend(asyncResult);

                logger.Debug(() => $"Sent {count} bytes to server.");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void Receive()
        {
            logger.Trace(() => nameof(Receive));

            try
            {
                var client = new ClientState(this.socket);

                client.Socket.BeginReceive(client.Buffer, 0, ClientState.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(CallbackReceive), client);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        #endregion

        protected class ClientState
        {
            public const int BUFFER_SIZE = 256;

            public byte[] Buffer { get; } = new byte[BUFFER_SIZE];

            public Socket Socket { get; }

            public ClientState(Socket socket)
            {
                this.Socket = socket;
            }
        }
    }
}
