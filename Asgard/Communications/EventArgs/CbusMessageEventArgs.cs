using System;
using Asgard.Data;

namespace Asgard.Communications
{
    public class CbusMessageEventArgs : EventArgs
    {
        public ICbusMessage Message { get; }

        public CbusMessageEventArgs(ICbusMessage message)
        {
            this.Message = message;
        }
    }
}
