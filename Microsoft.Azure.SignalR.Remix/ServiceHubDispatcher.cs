using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Azure.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.SignalR
{
    internal class ServiceHubDispatcher
    {
        // HACK: Get the dispatcher so that we can invoke it
        private readonly Type _serviceDispatcherType = typeof(ServiceOptions).Assembly.GetType("Microsoft.Azure.SignalR.ServiceHubDispatcher`1");
        private readonly IServiceProvider _serviceProvider;

        public ServiceHubDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Start(Type hubType, ConnectionDelegate app)
        {
            var type = _serviceDispatcherType.MakeGenericType(hubType);
            var startMethod = type.GetMethod("Start");

            object dispatcher = _serviceProvider.GetRequiredService(type);

            startMethod.Invoke(dispatcher, new object[] { app });
        }

    }
}
