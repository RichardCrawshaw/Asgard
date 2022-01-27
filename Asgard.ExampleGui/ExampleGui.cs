using System.Threading;
using System.Threading.Tasks;
using Asgard.Communications;
using Asgard.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asgard.ExampleGui
{
    public class ExampleGui : 
        IHostedService
    {
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ILogger<ExampleGui> logger;
        private readonly Controller controller;

        public ExampleGui(IHostApplicationLifetime hostApplicationLifetime,
                          ILogger<ExampleGui> logger,
                          Controller controller)
        {
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.logger = logger;
            this.controller = controller;

            this.hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
            this.hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
            this.hostApplicationLifetime.ApplicationStopped.Register(OnStopped);
        }

        private void OnStarted()
        {
            this.controller.Run();
        }

        private void OnStopping()
        {
            this.logger?.LogInformation("Application stopping.");
        }

        private void OnStopped()
        {
            this.logger?.LogInformation("Application stopped.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
