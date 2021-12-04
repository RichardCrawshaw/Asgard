using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    public interface ICbusMessenger
    {
        event EventHandler<CbusMessageEventArgs> MessageReceived;
        event EventHandler<CbusMessageEventArgs> MessageSent;

        Task<bool> SendMessage(ICbusMessage message);

        
    }
}
