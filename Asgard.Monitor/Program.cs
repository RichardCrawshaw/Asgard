﻿using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Asgard.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asgard.Monitor
{
    [SupportedOSPlatform("Linux")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("windows")]
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await
                Host
                    .CreateDefaultBuilder(args)
                    .ConfigureServices(ConfigureServices)
                    .RunConsoleAsync();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddAsgard(context.Configuration.GetSection("Asgard"));
            services.AddHostedService<AsgardMonitor>();
        }
    }
}
