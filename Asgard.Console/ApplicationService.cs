using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Asgard.Communications;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asgard.Console
{
    [SupportedOSPlatform("Linux")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("windows")]
    internal class ApplicationService : IHostedService
    {
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ICbusMessenger cbusMessenger;
        private readonly ILogger<ApplicationService> logger;

        public ApplicationService(IHostApplicationLifetime hostApplicationLifetime, ICbusMessenger cbusMessenger, ILogger<ApplicationService> logger)
        {
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.cbusMessenger = cbusMessenger;
            this.logger = logger;

            this.hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
            this.hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
            this.hostApplicationLifetime.ApplicationStopped.Register(OnStopped);
        }

        private void OnStarted()
        {
            //TODO: DI?
            var approot = new ApplicationRoot(this.cbusMessenger);
            approot.Start();
        }

        private void OnStopping()
        {
            this.logger?.LogInformation("Application stopping.");
            this.cbusMessenger?.Close();
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