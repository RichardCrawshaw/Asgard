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

            var frame = 
                options.Frame ?? 
                new CbusCanFrameSettings
                {
                    CanId = 125,
                    MajorPriority = Enum.GetName(MajorPriority.Low),
                    MinorPriority = Enum.GetName(MinorPriority.Normal),
                };

            var result =
                ActivatorUtilities.CreateInstance<CbusCanFrame>(
                    this.services, new object[] { frame, message });
            return result;
        }
    }
}
