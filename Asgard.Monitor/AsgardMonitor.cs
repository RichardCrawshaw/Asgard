﻿using System;
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
            this.logger?.LogTrace(nameof(OnStarted));
            await Task.Run(async () => await MainProcess());
        }

        private void OnStopping()
        {
            this.logger?.LogInformation("Application stopping.");
            this.cbusMessenger?.Close();
            if (this.cbusMessenger is not null)
            {
                this.cbusMessenger.StandardMessageReceived -= CbusMessenger_StandardMessageReceived;
                this.cbusMessenger.ExtendedMessageReceived -= CbusMessenger_ExtendedMessageReceived;
            }
        }

        private void OnStopped()
        {
            this.logger?.LogTrace(nameof(OnStopped));
            this.logger?.LogInformation("Application stopped.");
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
                this.logger.LogError("Error opening connection: {message}", e.Message);
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

        private static ConnectionOptions? SelectConnection(string[] connections)
        {
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
                    var connectionOptions = new ConnectionOptions
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
            this.cbusMessenger.StandardMessageReceived += CbusMessenger_StandardMessageReceived;
            this.cbusMessenger.ExtendedMessageReceived += CbusMessenger_ExtendedMessageReceived;

            await this.cbusMessenger.OpenAsync(connectionOptions);
        }

        private void CbusMessenger_StandardMessageReceived(object? sender, CbusStandardMessageEventArgs e)
        {
            if (e.Message.TryGetOpCode(out var opCode))
                Console.WriteLine($"Message received: {opCode}");
            else
                Console.WriteLine($"Unknown message: {e.Message?.ToString() ?? "null"}");
        }

        private void CbusMessenger_ExtendedMessageReceived(object? sender, CbusExtendedMessageEventArgs e)
        {
            this.logger?.LogTrace("ExtendedMessageReceived");
            Console.WriteLine($"Message received: {e.Message}");
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
