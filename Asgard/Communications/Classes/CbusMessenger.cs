using System;
using System.Threading.Tasks;
using Asgard.Data;
using Microsoft.Extensions.Logging;

namespace Asgard.Communications
{
    public class CbusMessenger : 
        ICbusMessenger
    {
        private IGridConnectProcessor? transport;

        private readonly ICbusCanFrameProcessor cbusCanFrameProcessor;
        private readonly ICbusConnectionFactory connectionFactory;
        private readonly ICbusCanFrameFactory cbusCanFrameFactory;
        private readonly ILogger<CbusMessenger>? logger;

        public event EventHandler<CbusMessageEventArgs>? MessageReceived;
        public event EventHandler<CbusMessageEventArgs>? MessageSent;
        
        public bool IsOpen { get; private set; }

        public CbusMessenger(ICbusCanFrameProcessor cbusCanFrameProcessor, ICbusConnectionFactory connectionFactory, ICbusCanFrameFactory cbusCanFrameFactory, ILogger<CbusMessenger>? logger = null)
        {
            this.cbusCanFrameProcessor = cbusCanFrameProcessor;
            this.connectionFactory = connectionFactory;
            this.cbusCanFrameFactory = cbusCanFrameFactory;
            this.logger = logger;
        }
        public async Task OpenAsync(ConnectionOptions connectionOptions)
        {
            if (this.IsOpen) return;

            this.transport = connectionFactory.GetConnection(connectionOptions);
            this.transport.GridConnectMessage += HandleTransportMessage;
            await this.transport.OpenAsync();

            this.IsOpen = true;
        }
        public async Task OpenAsync()
        {
            if (this.IsOpen) return;

            this.transport = connectionFactory.GetConnection();
            this.transport.GridConnectMessage += HandleTransportMessage;
            await this.transport.OpenAsync();

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

        private void HandleTransportMessage(object? sender, MessageReceivedEventArgs e)
        {
            try
            {
                var frame = this.cbusCanFrameProcessor.ParseFrame(e.Message);
                this.logger?.LogTrace("Parsed received Message: {0}", frame);
                MessageReceived?.Invoke(this, new CbusMessageEventArgs(frame.Message, true));
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, @"Error parsing message ""{0}""", e.Message);
                //TODO: wrap exception?
                throw;
            }
        }
        
        /// <summary>
        /// Sends the specified <paramref name="message"/> asynchronously.
        /// </summary>
        /// <param name="message">An <see cref="ICbusMessenger"/> instance.</param>
        /// <returns>A <see cref="Task{TResult}"/> <see cref="bool"/> that resolves to true on success; false otherwise.</returns>
        /// <remarks>
        /// An overload of this method exists on <see cref="ICbusMessenger"/> that allows an 
        /// <see cref="ICbusOpCode"/> to be passed instead of the underlying message. See
        /// <seealso cref="ICbusMessenger.SendMessage(ICbusOpCode)"/>.
        /// </remarks>
        public async Task<bool> SendMessage(ICbusMessage message)
        {
            //Note: An overload of this method exists on ICbusMessenger that allows an ICbusOpcode
            //      to be passed instead of the underlying message.

            var cbusCanFrame = this.cbusCanFrameFactory.CreateFrame(message);

            this.logger?.LogTrace("Sending message: {0}", message);
            try
            {
                if (this.transport is null) return false;
                await this.transport.SendMessage(
                    this.cbusCanFrameProcessor.ConstructTransportString(cbusCanFrame));
            }
            catch (Exception ex)
            {
                this.logger?.LogWarning(ex, "Failed to send message: {0}", message);
                return false;
            }
            MessageSent?.Invoke(this, new CbusMessageEventArgs(message, false));
            return true;
        }

    }
}
