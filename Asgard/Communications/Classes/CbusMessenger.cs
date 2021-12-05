using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Data;

namespace Asgard.Communications
{
    public class CbusMessenger : ICbusMessenger
    {
        private readonly IGridConnectProcessor _transport;
        private readonly ICbusCanFrameProcessor cbusCanFrameProcessor;
        private readonly ILogger<CbusMessenger> _logger;
        public event EventHandler<CbusMessageEventArgs> MessageReceived;
        public event EventHandler<CbusMessageEventArgs> MessageSent;

        public CbusMessenger(IGridConnectProcessor transport, ICbusCanFrameProcessor cbusCanFrameProcessor, ILogger<CbusMessenger> logger = null)
        {
            _transport = transport;
            this.cbusCanFrameProcessor = cbusCanFrameProcessor;
            _logger = logger;
            _transport.GridConnectMessage += HandleTransportMessage;
        }

        private void HandleTransportMessage(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                var frame = cbusCanFrameProcessor.ParseFrame(e.Message);
                _logger?.LogTrace("Parsed received Message: {0}", frame);
                MessageReceived?.Invoke(this, new CbusMessageEventArgs(frame.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, @"Error parsing message ""{0}""", e.Message);
                //TODO: wrap exception?
                throw;
            }
        }

        public async Task<bool> SendMessage(ICbusMessage message)
        {
            //TODO: consider cbuscanframe factory to decouple this
            var frame = new CbusCanFrame();
            //TODO: make configurable
            frame.CanId = 125;
            frame.MinorPriority = MinorPriority.Normal;
            frame.MajorPriority = MajorPriority.Low;
            frame.Message = message;

            _logger?.LogTrace("Sending message: {0}", message);
            await _transport.SendMessage(cbusCanFrameProcessor.ConstructTransportString(frame));
            MessageSent?.Invoke(this, new CbusMessageEventArgs(message));
            return true;
        }

    }
}
