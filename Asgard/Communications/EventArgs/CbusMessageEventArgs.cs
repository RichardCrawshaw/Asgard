using System;
using Asgard.Data;

namespace Asgard.Communications
{
    public class CbusMessageEventArgs : EventArgs
    {
        public ICbusMessage? Message { get; }

        public bool Received { get; }

        public CbusMessageEventArgs(ICbusMessage? message, bool received)
        {
            this.Message = message;
            this.Received = received;
        }
    }

    public class CbusStandardMessageEventArgs : CbusMessageEventArgs
    {
        public new ICbusStandardMessage Message { get; }

        public CbusStandardMessageEventArgs(ICbusStandardMessage message, bool received)
            : base(message, received)
        {
            this.Message = message;
        }
    }

    public class CbusExtendedMessageEventArgs : CbusMessageEventArgs
    {
        public new ICbusExtendedMessage Message { get; }

        public CbusExtendedMessageEventArgs(ICbusExtendedMessage message, bool received)
            : base(message, received)
        {
            this.Message= message;
        }
    }
}
