using System;
using System.Net.Sockets;

namespace Asgard
{
    public class SocketClosedEventArgs : EventArgs
    {
        public Socket Socket { get; }

        public SocketClosedEventArgs(Socket socket) => this.Socket = socket;
    }
}
