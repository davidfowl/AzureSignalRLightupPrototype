using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: HostingStartup(typeof(AzureSignalRHostingStartup))]

namespace Microsoft.Azure.SignalR
{
    public class AzureSignalRHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                if (!context.HostingEnvironment.IsDevelopment() || context.Configuration.GetSection("Azure:SignalR:Enabled").Get<bool>())
                {
                    services.AddSignalR().AddAzureSignalR2();
                }
            });
        }
    }
}
