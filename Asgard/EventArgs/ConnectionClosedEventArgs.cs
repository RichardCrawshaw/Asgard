﻿using System;
using System.Net.Sockets;

namespace Asgard
{
    public class ConnectionClosedEventArgs : EventArgs
    {
        public Socket Socket { get; }

        public ConnectionClosedEventArgs(Socket socket) => this.Socket = socket;
    }
}
