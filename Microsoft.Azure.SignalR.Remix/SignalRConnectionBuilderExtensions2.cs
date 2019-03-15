using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace Microsoft.AspNetCore.SignalR
{
    public static class SignalRConnectionBuilderExtensions2
    {
        private static readonly MethodInfo _useHubMethod = typeof(SignalRConnectionBuilderExtensions).GetMethod(nameof(SignalRConnectionBuilderExtensions.UseHub));

        // A late bount version of UseHub<T>
        public static IConnectionBuilder UseHub(this IConnectionBuilder builder, Type hubType)
        {
            return (IConnectionBuilder)_useHubMethod.MakeGenericMethod(hubType).Invoke(null, new object[] { builder });
        }
    }
}
