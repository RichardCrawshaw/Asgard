using System;
using System.Collections.Generic;
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

        public string[] GetAvailableConnections() => SerialPortTransport.GetPorts();

        public IGridConnectProcessor GetConnection()
        {
            return GetConnection(this.options.CurrentValue);
        }

        public IGridConnectProcessor GetConnection(ConnectionOptions connectionOptions)
        {
            ITransport transport = connectionOptions.ConnectionType switch
            {
                ConnectionOptions.ConnectionTypes.SerialPort
                when connectionOptions.SerialPort is null =>
                    throw new Exception("Serial port is selected but not defined."),
                ConnectionOptions.ConnectionTypes.SerialPort => 
                    ActivatorUtilities.CreateInstance<SerialPortTransport>(
                        this.services, new[] { connectionOptions.SerialPort }),
                _ => throw new Exception($"Unknown connection type: {connectionOptions.ConnectionType}."),
            };

            return 
                ActivatorUtilities.CreateInstance<GridConnectProcessor>(
                    this.services, new[] { transport });
        }
    }
}
