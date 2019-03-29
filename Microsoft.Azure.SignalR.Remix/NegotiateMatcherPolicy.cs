using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Common;

namespace Microsoft.Azure.SignalR.Remix
{
    internal class NegotiateMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        // This caches the replacement endpoints for negotiate so they are not recomputed on every request
        private readonly ConcurrentDictionary<Endpoint, Endpoint> _negotiateEndpointCache = new ConcurrentDictionary<Endpoint, Endpoint>();

        public override int Order => 1;

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            for (int i = 0; i < endpoints.Count; i++)
            {
                var endpoint = endpoints[i];
                var hubMetadata = endpoint.Metadata.GetMetadata<HubMetadata>();

                // REVIEW: Should we add metadata for the negotiate endpoint to make this a bit cleaner?
                if (hubMetadata != null && endpoint is RouteEndpoint route && route.RoutePattern.RawText.EndsWith("/negotiate"))
                {
                    return true;
                }
            }

            return false;
        }

        public Task ApplyAsync(HttpContext httpContext, EndpointSelectorContext context, CandidateSet candidates)
        {
            for (var i = 0; i < candidates.Count; i++)
            {
                ref var candidate = ref candidates[i];
                var endpoint = candidate.Endpoint as RouteEndpoint;

                var newEndpoint = _negotiateEndpointCache.GetOrAdd(candidate.Endpoint, CreateNegotiateEndpoint);

                candidates.ReplaceEndpoint(i, newEndpoint, candidate.Values);
            }

            return Task.CompletedTask;
        }

        private static Endpoint CreateNegotiateEndpoint(Endpoint endpoint)
        {
            Debug.Assert(endpoint is RouteEndpoint, "Not a route end point!");

            var routeEndpoint = (RouteEndpoint)endpoint;
            var hubMetadata = endpoint.Metadata.GetMetadata<HubMetadata>();

            // This replaces the negotiate endpoint with one that does the service redirect

            var routeEndpointBuilder = new RouteEndpointBuilder(async context =>
            {
                var handler = new NegotiateHandler(context.RequestServices);

                NegotiationResponse negotiateResponse = null;
                try
                {
                    negotiateResponse = handler.Process(context, hubMetadata.HubType.Name);
                }
                catch (AzureSignalRAccessTokenTooLongException ex)
                {
                    // Log.NegotiateFailed(_logger, ex.Message);
                    context.Response.StatusCode = 413;
                    await context.Response.WriteAsync(ex.Message);
                    return;
                }

                var writer = new MemoryBufferWriter();
                try
                {
                    context.Response.ContentType = "application/json";
                    NegotiateProtocol.WriteResponse(negotiateResponse, writer);
                    // Write it out to the response with the right content length
                    context.Response.ContentLength = writer.Length;
                    await writer.CopyToAsync(context.Response.Body);
                }
                finally
                {
                    writer.Reset();
                }
            },
            routeEndpoint.RoutePattern,
            routeEndpoint.Order);

            // Preserve the metadata
            foreach (var metadata in endpoint.Metadata)
            {
                routeEndpointBuilder.Metadata.Add(metadata);
            }

            return routeEndpointBuilder.Build();
        }
    }
}
