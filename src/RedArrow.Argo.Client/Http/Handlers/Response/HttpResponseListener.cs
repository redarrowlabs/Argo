using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Query;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RedArrow.Argo.Client.Http.Handlers.Response
{
    /// <summary>
    /// Defines listeners for JSON API Client responses with additional
    /// context such as the request and specific resource.  
    /// </summary>
    public abstract class HttpResponseListener
    {
        /// <summary>
        /// Called by requests for individual resources
        /// </summary>
        /// <param name="statusCode">Response status</param>
        /// <param name="id">Resource ID</param>
        /// <param name="resourceType">Resource Type</param>
        public virtual Task GetResource(HttpStatusCode statusCode, Guid id, string resourceType)
        {
            // Do nothing
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by requests for related resources
        /// </summary>
        /// <param name="statusCode">Response status</param>
        /// <param name="resourceId">Owner Resource ID</param>
        /// <param name="resourceType">Owner Resource Type</param>
        /// <param name="rltnName">Relationship Name</param>
        public virtual Task GetRelated(HttpStatusCode statusCode, Guid resourceId, string resourceType, string rltnName)
        {
            // Do nothing
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by requests to create a resource
        /// </summary>
        /// <param name="statusCode">Whether Argo considers the response code successful.</param>
        /// <param name="resource">Resource being created</param>
        public virtual Task CreateResource(HttpStatusCode statusCode, ResourceRootSingle resource)
        {
            // Do nothing
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by requests to update a resource
        /// </summary>
        /// <param name="statusCode">Response status</param>
        /// <param name="originalResource">Resource being updated</param>
        /// <param name="patch">Resource patch</param>
        public virtual Task UpdateResource(HttpStatusCode statusCode, Resource originalResource, ResourceRootSingle patch)
        {
            // Do nothing
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by requests to query resources
        /// </summary>
        /// <param name="statusCode">Response status</param>
        /// <param name="query">Query Context</param>
        public virtual Task QueryResources(HttpStatusCode statusCode, IQueryContext query)
        {
            // Do nothing
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by requests to delete resources
        /// </summary>
        /// <param name="statusCode">Response status</param>
        /// <param name="id">Resource ID</param>
        /// <param name="resourceType">Resource Type</param>
        public virtual Task DeleteResource(HttpStatusCode statusCode, Guid id, string resourceType)
        {
            // Do nothing
            return Task.CompletedTask;
        }
    }
}
