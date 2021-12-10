using Asgard.Communications;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Extensions
{
    public static class ServiceCollectionExtensions
    {
        //TODO: create proper options classes, prefer IOptions<T> etc to IConfiguration
        //see: https://docs.microsoft.com/en-us/dotnet/core/extensions/options-library-authors
        public static IServiceCollection AddMergCbus(this IServiceCollection services)
        {
            //TODO: just an example, need to figure out lifetimes correctly
            services.AddSingleton<Communications.IGridConnectProcessor, Communications.GridConnectProcessor>();

            //TODO: read config, pass in Can ID etc
            services.AddSingleton<ICbusMessenger, CbusMessenger>();

            services.AddScoped<ICbusConnectionFactory, CbusConnectionFactory>();
            services.AddScoped<SerialPortTransport>();

            services.AddSingleton<ICbusCanFrameProcessor, CbusCanFrameProcessor>();

            return services;
        }
    }
}
