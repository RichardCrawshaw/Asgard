using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asgard.Communications;
using Asgard.Data;
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
            if (e.Message is not ICbusStandardMessage standardMessage)
                return;
            if (standardMessage is not null)
            {
                if (standardMessage.TryGetOpCode(out var opCode))
                    Console.WriteLine($"Message received: {opCode}");
            }
            else
            {
                Console.WriteLine($"Unknown message: {e.Message?.ToString() ?? "null"}");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task MainProcess()
        {
            var version = GetType().Assembly.GetName().Version;
            Console.WriteLine($"AsgardMonitor :: CBUS traffic logger :: Version {version}");

            try
            {
                var connections = GetConnectionNames();
                if (connections is null) return;

                DisplayConnections(connections);

                var connectionOptions = SelectConnection(connections);
                if (connectionOptions is null) return;

                await StartAsync(connectionOptions);

                DoMenu();
            }
            catch (TransportException e)
            {
                this.logger.LogError("Error opening connection: {0}", e.Message);
            }
            finally
            {
                this.hostApplicationLifetime.StopApplication();
            }
        }

        private string[]? GetConnectionNames()
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
                return null;
            }

            return connections;
        }

        private static void DisplayConnections(string[] connections)
        {
            Console.WriteLine("The following connections are available; select one.");
            for (var i = 0; i < connections.Length; i++)
                Console.WriteLine($"{i}: {connections[i]}");
        }

        private ConnectionOptions? SelectConnection(string[] connections)
        {
            ConnectionOptions? connectionOptions = null;
            while (true)
            {
                Console.WriteLine("Enter the number and press <enter> or 'Q' to quit.");
                var selectionString = Console.ReadLine();
                if (selectionString?.Equals("Q", StringComparison.OrdinalIgnoreCase) ?? true)
                    return null;

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
                    return connectionOptions;
                }

                Console.WriteLine("Unrecognised selection.");
            }
        }

        private async Task StartAsync(ConnectionOptions connectionOptions)
        {
            this.cbusMessenger.MessageReceived += CbusMessenger_MessageReceived;

            await this.cbusMessenger.OpenAsync(connectionOptions);
        }

        private static void DoMenu()
        {
            while (true)
            {
                Console.WriteLine("C : Clear Screen | Q : Quit");

                var key = Console.ReadKey();
                switch (key.KeyChar)
                {
                    case 'c':
                    case 'C':
                        Console.Clear();
                        break;
                    case 'q':
                    case 'Q':
                        return;
                }
            }
        }
    }
}
