using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Asgard.Comms
{
    /// <summary>
    /// Class to provide <see cref="Socket"/> client capability
    /// </summary>
    public class AsyncSocketClient : BaseAsyncSocket,
        ISocketClientAdapter
    {
        #region Fields

        // Thread signals to synchronise between different threads.
        private readonly ManualResetEventSlim connectDone = new(false);
        private readonly ManualResetEventSlim receiveDone = new(false);

        /// <summary>
        /// The <see cref="ISettings"/> object.
        /// </summary>
        private readonly ISettings settings;

        /// <summary>
        /// The current <see cref="Socket"/>.
        /// </summary>
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
        public override bool IsConnected => this.socket?.Connected ?? false;

        #endregion

        #region Constructors

        public AsyncSocketClient(ISettings settings)
            : base()
        {
            this.settings = settings;

            var settingsNode = this.settings.Get<AsyncSocketClient, ClientSettings>();

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

            this.ConnectionClosed += AsyncSocketClient_ConnectionClosed;
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
        /// Connect to a server.
        /// </summary>
        public void Connect()
        {
            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket from the IP Address and Port.
                var remoteEndPoint = GetEndPoint();

                // Close the socket prior to getting a new one.
                CloseSocket(this.socket);

                // Create a TCP/IP socket.
                this.socket = GetSocket();

                // Connect to the remote endpoint.  
                this.socket.BeginConnect(remoteEndPoint, new AsyncCallback(ConnectCallback), this.socket);

                // Wait for the connection to be accepted.
                this.connectDone.Wait(this.Token);
            }
            catch (OperationCanceledException)
            {
                // No logging of cancellation required.
            }
            catch (Exception)
            {
                // Log exception.
            }
        }

        /// <summary>
        /// Disconnect from the connected server.
        /// </summary>
        public void Disconnect()
        {
            Cancel();
            try
            {
                if (!this.socket.Connected)
                    this.connectDone.Wait(this.Token);
                else
                    this.receiveDone.Wait(this.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception)
            {
                // Log exception.
            }
            finally
            {
                ShutdownSocket(this.socket);
            }
        }

        /// <summary>
        /// Send the specified <paramref name="data"/>.
        /// </summary>
        /// <param name="data">A <see cref="byte[]"/> containing the data to send.</param>
        /// <returns></returns>
        public override bool Send(byte[] data)
        {
            if (!this.IsConnected) return false;

            Send(this.socket, data);
            return true;
        }

        /// <summary>
        /// Send the specified <paramref name="text"/>.
        /// </summary>
        /// <param name="text">A <see cref="string"/> containing the text to send.</param>
        public override bool Send(string text)
        {
            if (!this.IsConnected) return false;

            Send(this.socket, text);
            return true;
        }

        #endregion

        #region Support routines

        /// <summary>
        /// A callback routine that handles incoming connections using the specified <paramref name="asyncResult"/>.
        /// </summary>
        /// <param name="asyncResult">The <see cref="IAsyncResult"/> instance to process.</param>
        private void ConnectCallback(IAsyncResult asyncResult)
        {
            try
            {
                // Retrieve the socket from the state object.  
                if (asyncResult.AsyncState is not Socket client)
                {
                    return;
                }

                // Complete the connection.  
                client.EndConnect(asyncResult);

                // Signal that the connection has been made.  
                this.connectDone.Set();

                // Initiate receiving data...
                Read(client);
            }
            catch (Exception)
            {
                // Log the exception.
            }
        }

        #endregion

        #region Event hander routines

        /// <summary>
        /// The event handler routine for when a connected <see cref="Socket"/> is closed.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The <see cref="ConnectionClosedEventArgs"/> data.</param>
        private void AsyncSocketClient_ConnectionClosed(object sender, ConnectionClosedEventArgs e)
        {
            if (e?.Socket is not null && e.Socket == this.socket)
                Disconnect();
        }

        #endregion
    }
}