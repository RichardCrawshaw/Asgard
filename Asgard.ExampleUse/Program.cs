using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using Asgard;
using Asgard.Extensions;
using Asgard.Communications;
using Microsoft.Extensions.Logging;

namespace Asgard.ExampleUse
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // In a typical ASP.NET Core application, this will be provided to you
            var serviceProvider = new ServiceCollection();

            serviceProvider.AddMergCbus();
            serviceProvider.AddLogging();
            serviceProvider.AddTransient<ExampleUse>();

            var services = serviceProvider.BuildServiceProvider();
            var example = services.GetService<ExampleUse>();
            example.Start();

            Console.ReadKey();
        }
    }

    public class ExampleUse
    {
        private readonly ICbusMessenger cbusMessenger;
        private readonly ILogger<ExampleUse> logger;

        public ExampleUse(ICbusMessenger cbusMessenger, ILogger<ExampleUse> logger)
        {
            //TODO: needs a way to ensure comms are open etc.
            this.cbusMessenger = cbusMessenger;
            this.logger = logger;
        }
        public void Start()
        {
            cbusMessenger.MessageReceived += (sender, e) =>
            {
                logger.LogInformation($"Message received: {e.Message.GetOpCode()}");
            };
        }
    }
}
