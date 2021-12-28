using System;
using Asgard.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asgard.Communications
{
    internal class CbusCanFrameFactory :
        ICbusCanFrameFactory
    {
        private readonly IServiceProvider services;
        private readonly IOptionsMonitor<CbusCanFrameOptions> options;

        public CbusCanFrameFactory(IServiceProvider services,
                                   IOptionsMonitor<CbusCanFrameOptions> options)
        {
            this.services = services;
            this.options = options;
        }

        public ICbusCanFrame CreateFrame(ICbusMessage message)
        {
            var options = this.options.CurrentValue;

            var result =
                new CbusCanFrame(options.Frame, null);
                //ActivatorUtilities.CreateInstance<CbusCanFrame>(
                //    this.services, new[] { options.Frame, });
            result.Instantiate(message.GetOpCode());
            result.Message = message;
            return result;
        }
    }
}
