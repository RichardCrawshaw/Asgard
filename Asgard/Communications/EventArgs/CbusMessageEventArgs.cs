using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    public class CbusMessageEventArgs : EventArgs
    {
        public ICbusMessage Message { get; }

        public CbusMessageEventArgs(ICbusMessage message)
        {
            Message = message;
        }
    }
}
