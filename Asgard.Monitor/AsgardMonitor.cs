using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asgard.Communications;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asgard.Monitor
{
    internal class AsgardMonitor :
        IHostedService
    {
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ICbusMessenger cbusMessenger;
        private readonly ILogger<AsgardMonitor> logger;

        public AsgardMonitor(IHostApplicationLifetime hostApplicationLifetime,
                             ICbusMessenger cbusMessenger,
                             ILogger<AsgardMonitor> logger)
        {
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.cbusMessenger = cbusMessenger;
            this.logger = logger;

            this.hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
            this.hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
            this.hostApplicationLifetime.ApplicationStopped.Register(OnStopped);
        }

        private async void OnStarted()
        {
            await Task.Run(async () => await MainProcess());
        }

        private void OnStopping()
        {
            this.logger?.LogInformation("Application stopping.");
            this.cbusMessenger?.Close();
            if (this.cbusMessenger is not null)
                this.cbusMessenger.MessageReceived -= CbusMessenger_MessageReceived;
        }

        private void OnStopped()
        {
            this.logger?.LogInformation("Application stopped.");
        }

        private void CbusMessenger_MessageReceived(object? sender, CbusMessageEventArgs e)
        {
            if (e.Message is not null)
            {
                var opcode = e.Message.GetOpCode();
                this.logger?.LogInformation($"Message received: {opcode}");
            }
            else if (e.GridConnectMessage is not null)
                this.logger?.LogInformation($"GridConnect message: {e.GridConnectMessage}");
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task MainProcess()
        {
            Console.WriteLine("AsgardMonitor :: CBUS traffic logger");

            var quit = true;

            try
            {
                var connections =
                    this.cbusMessenger.GetAvailableConnections()
                        .OrderBy(n => n)
                        .ToArray();
                if (connections.Length == 0)
                {
                    Console.WriteLine("No connections available.");
                    Console.WriteLine("Establish a physical connections and try again.");
                    Console.WriteLine("Press <enter> to close.");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("The following connections are available; select one.");
                for (var i = 0; i < connections.Length; i++)
                    Console.WriteLine($"{i}: {connections[i]}");

                ConnectionOptions? connectionOptions = null;
                do
                {
                    Console.WriteLine("Enter the number and press <enter> or 'Q' to quit.");
                    var selectionString = Console.ReadLine();
                    if (selectionString?.Equals("Q", StringComparison.OrdinalIgnoreCase) ?? true) return;

                    if (int.TryParse(selectionString, out var selectionNumber) &&
                        selectionNumber >= 0 &&
                        selectionNumber < connections.Length)
                    {
                        connectionOptions = new ConnectionOptions
                        {
                            ConnectionType = ConnectionOptions.ConnectionTypes.SerialPort,
                            SerialPort = new SerialPortTransportSettings
                            {
                                PortName = connections[selectionNumber],
                            },
                        };
                    }
                    else
                    {
                        Console.WriteLine("Unrecognised selection.");
                    }
                } while (connectionOptions is null);

                if (connectionOptions is null) return;

                this.cbusMessenger.MessageReceived += CbusMessenger_MessageReceived;

                await this.cbusMessenger.OpenAsync(connectionOptions);

                Console.WriteLine("Press <enter> to exit.");
                Console.ReadLine();
            }
            catch (TransportException e)
            {
                this.logger.LogError("Error opening connection: {0}", e.Message);
            }
            finally
            {
                if (quit)
                    this.hostApplicationLifetime.StopApplication();
            }
        }
    }
}
