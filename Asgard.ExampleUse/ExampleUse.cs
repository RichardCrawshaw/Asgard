using System;
using System.Threading;
using System.Threading.Tasks;
using Asgard.Communications;
using Asgard.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asgard.ExampleUse
{
    public class ExampleUse : 
        IHostedService
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
                await this.cbusMessenger.OpenAsync();
                var mm = new MessageManager(this.cbusMessenger);

                //Send and receive replies by specifying what you're expecting in response to a request
                this.logger.LogInformation("Sending node query");
                var replies = await mm.SendMessageWaitForReplies<ResponseToQueryNode>(new QueryNodeNumber());
                foreach(var reply in replies)
                {
                    this.logger.LogInformation(
                        $"Node info: Node number: {reply.NodeNumber}, ModuleID: {reply.ModuleId}");
                }

                
                //Send and receive replies using built-in Asgard response filtering
                var response = await mm.SendMessageWaitForReply(new GetEngineSession());
                switch (response)
                {
                    case ResponseToQueryNode report:
                        //do stuff
                        break;
                    case CommandStationErrorReport error:
                        //do other stuff
                        break;
                }
                if (response is IErrorReplyTo<GetEngineSession>)
                {
                    //Do stuff if it was a en error reply without knowing specific response type as above
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
            this.cbusMessenger?.Close();
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
