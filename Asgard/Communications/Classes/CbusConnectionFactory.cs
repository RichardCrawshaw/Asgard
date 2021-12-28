using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asgard.Communications
{
    internal class CbusConnectionFactory : 
        ICbusConnectionFactory
    {
        private readonly IServiceProvider services;
        private readonly IOptionsMonitor<ConnectionOptions> options;

        public CbusConnectionFactory(IServiceProvider services,
                                     IOptionsMonitor<ConnectionOptions> options)
        {
            this.services = services;
            this.options = options;
        }

        public IGridConnectProcessor GetConnection()
        {
            var options = this.options.CurrentValue;
            ITransport transport = options.ConnectionType switch
            {
                ConnectionOptions.ConnectionTypes.SerialPort => 
                    ActivatorUtilities.CreateInstance<SerialPortTransport>(
                        this.services, new[] { options.SerialPort }),
                _ => throw new Exception("Unknown connection type"),
            };

            return 
                ActivatorUtilities.CreateInstance<GridConnectProcessor>(
                    this.services, new[] { transport });
        }
    }
}
