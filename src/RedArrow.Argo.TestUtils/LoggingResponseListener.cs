using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Http.Handlers.Response;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Query;
using Serilog;

namespace RedArrow.Argo.TestUtils
{
    public class LoggingResponseListener : HttpResponseListener
    {
        private ILogger Logger { get; }
        public LoggingResponseListener(ILogger logger)
        {
            Logger = logger;
        }

        public override Task CreateResource(HttpStatusCode statusCode, ResourceRootSingle resource)
        {
            Logger.Information("Created {Type}. Status: {StatusCode}", resource.Data.Type, statusCode);
            return Task.CompletedTask;
        }

        public override Task UpdateResource(HttpStatusCode statusCode, Resource originalResource, ResourceRootSingle patch)
        {
            Logger.Information("Patched {Id}. Status: {StatusCode}", patch.Data.Id, statusCode);
            return Task.CompletedTask;
        }

        public override Task DeleteResource(HttpStatusCode statusCode, Guid id, string resourceType)
        {
            Logger.Information("Deleted {Id}. Status: {StatusCode}", id, statusCode);
            return Task.CompletedTask;
        }

        public override Task GetResource(HttpStatusCode statusCode, Guid id, string resourceType)
        {
            Logger.Information("Got Resource {Id}. Status: {StatusCode}", id, statusCode);
            return Task.CompletedTask;
        }

        public override Task GetRelated(HttpStatusCode statusCode, Guid resourceId, string resourceType, string rltnName)
        {
            Logger.Information("Got Related {Id} {Relation}. Status: {StatusCode}", resourceId, rltnName, statusCode);
            return Task.CompletedTask;
        }

        public override Task QueryResources(HttpStatusCode statusCode, IQueryContext query)
        {
            Logger.Information("Queried type {Type} with filter {Filter}. Status: {StatusCode}", query.BasePath, query.Filters, statusCode);
            return Task.CompletedTask;
        }
    }
}
