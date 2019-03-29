using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.SignalR;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class AzureSignalRStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                // We need to call this today because it throws if we didn't call this and called AddAzureSignalR
                // That needs to be changed.
                app.UseAzureSignalR(r => { });

                next(app);

                // This can't be a hosted service because it needs to run after startup
                var service = app.ApplicationServices.GetRequiredService<AzureSignalRHostedService>();
                service.Start();
            };
        }
    }
}
