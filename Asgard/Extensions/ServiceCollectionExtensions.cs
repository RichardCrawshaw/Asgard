using Asgard.Communications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Asgard.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAsgard(this IServiceCollection services, IConfiguration configuration)
        {
            
            services.AddSingleton<ICbusMessenger, CbusMessenger>();
            services.AddSingleton<ICbusConnectionFactory, CbusConnectionFactory>();
            services.AddSingleton<ICbusCanFrameFactory, CbusCanFrameFactory>();
            services.AddScoped<ITransport, SerialPortTransport>();
            services.AddScoped<IGridConnectProcessor, GridConnectProcessor>();

            services.AddScoped<SerialPortTransportSettings>();
            services.AddScoped<CbusCanFrameSettings>();

            services.AddSingleton<ICbusCanFrameProcessor, CbusCanFrameProcessor>();

            services.Configure<ConnectionOptions>(configuration.GetSection("Connection"));
            services.Configure<CbusCanFrameOptions>(configuration.GetSection("CanFrame"));

            return services;
        }
    }
}
