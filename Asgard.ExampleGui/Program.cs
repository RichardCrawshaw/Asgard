using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Asgard.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asgard.ExampleGui
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var host = await
                Host.CreateDefaultBuilder(args)
                    .ConfigureServices(ConfigureServices)
                    .StartAsync();

            var view = host.Services.GetRequiredService<MainForm>();
            view.FormClosed += async (s,e) => await host.StopAsync();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services
                .AddLogging(configure => configure.AddConsole())
                .AddAsgard(context.Configuration.GetSection("Asgard"))
                .AddHostedService<ExampleGui>()
                .AddScoped<Controller>()
                .AddScoped<MainForm>();
        }
    }
}
