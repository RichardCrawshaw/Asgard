using System;
using System.Threading.Tasks;
using Asgard.Data;
using Microsoft.Extensions.Logging;

namespace Asgard.Communications
{
    public class CbusMessenger : 
        ICbusMessenger
    {
        private IGridConnectProcessor transport;

        private readonly ICbusCanFrameProcessor cbusCanFrameProcessor;
        private readonly ICbusConnectionFactory connectionFactory;
        private readonly ILogger<CbusMessenger> logger;

        public event EventHandler<CbusMessageEventArgs> MessageReceived;
        public event EventHandler<CbusMessageEventArgs> MessageSent;
        
        public bool IsOpen { get; private set; }

        public CbusMessenger(ICbusCanFrameProcessor cbusCanFrameProcessor, ICbusConnectionFactory connectionFactory, ILogger<CbusMessenger> logger = null)
        {
            this.cbusCanFrameProcessor = cbusCanFrameProcessor;
            this.connectionFactory = connectionFactory;
            this.logger = logger;
        }

        public void Open()
        {
            if (this.IsOpen) return;

            this.transport = connectionFactory.GetConnection();
            this.transport.GridConnectMessage += HandleTransportMessage;
            this.transport.Open();

            this.IsOpen = true;
        }

        public void Close()
        {
            // Don't test the IsOpen flag, as it won't be set if the Open method hasn't completed.

            if (this.transport?.Close() ?? false)
                this.transport.GridConnectMessage -= HandleTransportMessage;
            this.transport = null;

            this.IsOpen = false;
        }

        private void HandleTransportMessage(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                var frame = this.cbusCanFrameProcessor.ParseFrame(e.Message);
                this.logger?.LogTrace("Parsed received Message: {0}", frame);
                MessageReceived?.Invoke(this, new CbusMessageEventArgs(frame.Message));
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, @"Error parsing message ""{0}""", e.Message);
                //TODO: wrap exception?
                throw;
            }
        }
        
        public async Task<bool> SendMessage(ICbusMessage message)
        {
            //Note: An overload of this method exists on ICbusMessenger that allows an ICbusOpcode to be 
            //      passed instead of the underlying message.

            //TODO: consider cbuscanframe factory to decouple this
            var frame = new CbusCanFrame();
            //TODO: make configurable
            frame.CanId = 125;
            frame.MinorPriority = MinorPriority.Normal;
            frame.MajorPriority = MajorPriority.Low;
            frame.Message = message;

            this.logger?.LogTrace("Sending message: {0}", message);
            await this.transport.SendMessage(
                this.cbusCanFrameProcessor.ConstructTransportString(frame));
            MessageSent?.Invoke(this, new CbusMessageEventArgs(message));
            return true;
        }

    }
}
