using System;

namespace Asgard.Comms
{
    public class SerialProcessorEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}