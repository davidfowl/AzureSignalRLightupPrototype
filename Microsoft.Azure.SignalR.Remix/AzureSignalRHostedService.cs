using System;
using System.Linq;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;

namespace Microsoft.Azure.SignalR
{
    public class AzureSignalRHostedService
    {
        private readonly EndpointDataSource _dataSource;
        private readonly IServiceProvider _serviceProvider;

        public AzureSignalRHostedService(EndpointDataSource dataSource, IServiceProvider serviceProvider)
        {
            _dataSource = dataSource;
            _serviceProvider = serviceProvider;
        }

        public void Start()
        {
            // Get a list of all registered hubs
            var hubTypes = _dataSource.Endpoints.Select(e => e.Metadata.GetMetadata<HubMetadata>()?.HubType)
                                   .Where(hubType => hubType != null)
                                   .Distinct()
                                   .ToList();

            // Make late bound version of the hub dispatcher
            var dispatcher = new ServiceHubDispatcher(_serviceProvider);

            foreach (var hubType in hubTypes)
            {
                // Start the application for each of the hub types
                var app = new ConnectionBuilder(_serviceProvider)
                                .UseHub(hubType)
                                .Build();

                dispatcher.Start(hubType, app);
            }
        }
    }
}
