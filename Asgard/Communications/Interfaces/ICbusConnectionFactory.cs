using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    public interface ICbusConnectionFactory
    {
        //TODO: pass in settings
        IGridConnectProcessor GetConnection();
    }
}
