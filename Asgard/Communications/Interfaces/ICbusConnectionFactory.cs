using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    public interface ICbusConnectionFactory
    {
        IGridConnectProcessor GetConnection();
        IGridConnectProcessor GetConnection(ConnectionOptions connectionOptions);
    }
}
