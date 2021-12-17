using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    internal class CbusConnectionFactory:ICbusConnectionFactory
    {
        private readonly IServiceProvider services;
        private readonly IOptionsMonitor<ConnectionOptions> options;

        public CbusConnectionFactory(IServiceProvider services, IOptionsMonitor<ConnectionOptions> options)
        {
            this.services = services;
            this.options = options;
        }

        public IGridConnectProcessor GetConnection()
        {
            var opt = options.CurrentValue;
            ITransport transport = opt.ConnectionType switch
            {
                ConnectionOptions.ConnectionTypes.SerialPort => ActivatorUtilities.CreateInstance<SerialPortTransport>(services, new[] { opt.SerialPort }),
                _ => throw new Exception("Unknown connection type"),
            };
            return ActivatorUtilities.CreateInstance<GridConnectProcessor>(services, new[] { transport });
        }
    }
}
