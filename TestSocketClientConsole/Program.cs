using System;
using System.Linq;
using System.Text;
using System.Threading;
using Asgard;
using NLog;

namespace TestSocketClientConsole
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static void Main()
        {
            logger.Info(() => nameof(TestSocketClientConsole));

            var settings = new Settings();

            var client = new AsyncSocketClient(settings);
            client.DataReceived += Client_DataReceived;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Console.WriteLine("Connecting to server...");
                client.Connect();
                if (client.IsConnected)
                    Console.WriteLine("Connected to server.");
            });
            Console.WriteLine("Enter some text to send then press enter.");
            Console.WriteLine("Enter a blank line to quit.");

            var text = Console.ReadLine();
            while (!string.IsNullOrEmpty(text) && client.IsConnected)
            {
                if (!client.Send(text))
                    Console.WriteLine("Failed to send!");
                if (!client.IsConnected)
                    break;
                text = Console.ReadLine();
            }

            Console.WriteLine("Disconnecting from server...");
            client.Disconnect();
            Console.WriteLine("Disconnected from server.");

            Console.WriteLine("Press enter to quit...");
            Console.ReadLine();
        }

        private static void Client_DataReceived(object sender, DataReceivedEventArgs e)
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
