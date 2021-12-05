using System;
using System.Linq;
using System.Text;
using Asgard;
using Asgard.Comms;
using NLog;

namespace TestSocketListenerConsole
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static void Main()
        {
            logger.Info(() => nameof(TestSocketListenerConsole));

            var settings = new Settings();

            var server = new AsyncSocketServer(settings);
            server.DataReceived += Server_DataReceived;

            Console.WriteLine("Starting server...");
            server.Start();
            Console.WriteLine("Server started.");

            Console.WriteLine("Enter some text to send then press enter.");
            Console.WriteLine("Enter a blank line to quit.");

            var text = Console.ReadLine();
            while (!string.IsNullOrEmpty(text))
            {
                if (!server.Send(text))
                    Console.WriteLine("Failed to send!");
                text = Console.ReadLine();
            }

            Console.WriteLine("Stopping server...");
            server.Stop();
            Console.WriteLine("Server stopped.");

            Console.WriteLine("Press Enter to quit.");
            Console.ReadLine();
        }

        private static void Server_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if ((e.Data?.Length ?? 0) > 0)
            {
                var text = Encoding.ASCII.GetString(e.Data);
                logger.Debug(() => text);
                Console.WriteLine($">>> {text}");

                text = string.Join(" ", e.Data.Select(b => $"0X{b:X2}"));
                logger.Debug(() => text);
                Console.WriteLine($">>> {text}");
            }
        }
    }
}
