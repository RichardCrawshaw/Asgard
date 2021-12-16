using Asgard.Communications;
using Microsoft.Extensions.Configuration;
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
        public static IServiceCollection AddAsgard(this IServiceCollection services, IConfiguration configuration)
        {
            
            services.AddScoped<ICbusMessenger, CbusMessenger>();
            services.AddScoped<ICbusConnectionFactory, CbusConnectionFactory>();
            services.AddScoped<SerialPortTransport>();
            services.AddScoped<GridConnectProcessor>();

            services.AddSingleton<ICbusCanFrameProcessor, CbusCanFrameProcessor>();

            services.Configure<ConnectionOptions>(configuration.GetSection("Connection"));

            return services;
        }
    }
}
