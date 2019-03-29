using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR;
using Microsoft.Azure.SignalR.Remix;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SignalRAzureExtensions
    {
        public static ISignalRServerBuilder AddAzureSignalR2(this ISignalRServerBuilder builder)
        {
            builder.AddAzureSignalR();
            builder.Services.TryAddSingleton<AzureSignalRHostedService>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IStartupFilter, AzureSignalRStartupFilter>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, NegotiateMatcherPolicy>());
            return builder;
        }
    }
}
