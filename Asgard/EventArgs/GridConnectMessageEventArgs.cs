using System;

namespace Asgard.Comms
{
    public class GridConnectMessageEventArgs : EventArgs
    {
        public string Message { get; set; }

        public byte[] Payload { get; set; }
    }
}
