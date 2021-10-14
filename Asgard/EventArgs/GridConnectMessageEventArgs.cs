using System;

namespace Asgard
{
    internal class GridConnectMessageEventArgs : EventArgs
    {
        public string Message { get; set; }

        public byte[] Payload { get; set; }
    }
}
