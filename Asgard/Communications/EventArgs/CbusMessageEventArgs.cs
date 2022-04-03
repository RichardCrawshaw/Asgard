using System;
using Asgard.Data;

namespace Asgard.Communications
{
    public class CbusMessageEventArgs : EventArgs
    {
        public ICbusMessage? Message { get; }

        public bool Received { get; }

        public CbusMessageEventArgs(ICbusMessage? message,
                                    bool received)
        {
            this.Message = message;
            this.Received = received;
        }
    }
}
