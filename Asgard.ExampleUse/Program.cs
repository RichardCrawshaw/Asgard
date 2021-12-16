using Microsoft.Extensions.DependencyInjection;
using Asgard.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Asgard.ExampleUse
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .RunConsoleAsync();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddAsgard(context.Configuration.GetSection("Asgard"));
            services.AddHostedService<ExampleUse>();
        }
    }
}
