using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Communications
{
    internal class CbusConnectionFactory:ICbusConnectionFactory
    {
        private readonly IServiceScope services;

        public CbusConnectionFactory(IServiceScope services)
        {
            this.services = services;
        }

        public IGridConnectProcessor GetConnection()
        {
            //TODO: use settings to find appropriate connection
            var transport =  services.ServiceProvider.GetRequiredService<SerialPortTransport>();

            return ActivatorUtilities.CreateInstance<IGridConnectProcessor>(services.ServiceProvider, new[] { transport });
        }
    }
}
