using System;
using Asgard.Data;

namespace Asgard.Communications
{
    public class CbusMessageEventArgs : EventArgs
    {
        public ICbusMessage? Message { get; }
        public string? GridConnectMessage { get; }
        public bool Received { get; }

        public CbusMessageEventArgs(ICbusMessage? message, string? gridConnectMessage, bool received)
        {
            this.Message = message;
            this.GridConnectMessage = gridConnectMessage;
            this.Received = received;
        }
    }
}
