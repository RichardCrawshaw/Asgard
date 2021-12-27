﻿using Asgard.Communications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using System;
using Asgard.Data;

namespace Asgard.ExampleUse
{
    public class ExampleUse:IHostedService
    {
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ICbusMessenger cbusMessenger;
        private readonly ILogger<ExampleUse> logger;

        public ExampleUse(IHostApplicationLifetime hostApplicationLifetime, ICbusMessenger cbusMessenger, ILogger<ExampleUse> logger)
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
            this.cbusMessenger.MessageReceived += CbusMessenger_MessageReceived;

            try
            {
                this.cbusMessenger.Open();

                var mm = new MessageManager(this.cbusMessenger);
                this.logger.LogInformation("Sending node query");
                var replies = await mm.SendMessageWaitForReplies<ResponseToQueryNode>(new QueryNodeNumber());
                foreach(var reply in replies)
                {
                    this.logger.LogInformation(
                        $"Node info: Node number: {reply.NodeNumber}, ModuleID: {reply.ModuleId}");
                }
            }
            catch (TransportException e)
            {
                this.logger.LogError("Error opening connection: {0}", e.Message);
            }
        }

        private void OnStopping()
        {
            this.logger?.LogInformation("Application stopping.");
            if (this.cbusMessenger is not null)
                this.cbusMessenger.MessageReceived -= CbusMessenger_MessageReceived;
        }

        private void OnStopped()
        {
            this.logger?.LogInformation("Application stopped.");
        }

        private void CbusMessenger_MessageReceived(object sender, CbusMessageEventArgs e)
        {
            this.logger.LogInformation($"Message received: {e.Message.GetOpCode()}");
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
