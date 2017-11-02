using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Query;
using System;
using System.Net.Http;

namespace RedArrow.Argo.Client.Http.Handlers.Request
{
    /// <summary>
    /// Defines interceptors for Argo http requests with additional context, allowing
    /// the implementer to change the request before it is sent.  One example might be
    /// shuffling metadata like ETags between the resource and the headers.
    /// </summary>
    public abstract class HttpRequestModifier
    {
        /// <summary>
        /// Modifies requests for individual resources
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id">Resource ID</param>
        /// <param name="resourceType">Resource Type</param>
        public virtual void GetResource(HttpRequestMessage request, Guid id, string resourceType)
        {
            // Do nothing
        }

        /// <summary>
        /// Modifies requests for related resources
        /// </summary>
        /// <param name="request"></param>
        /// <param name="resourceId">Owner Resource ID</param>
        /// <param name="resourceType">Owner Resource Type</param>
        /// <param name="rltnName">Relationship Name</param>
        public virtual void GetRelated(HttpRequestMessage request, Guid resourceId, string resourceType, string rltnName)
        {
            // Do nothing
        }

        /// <summary>
        /// Modifies requests to create a resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="resource">Resource being created</param>
        public virtual void CreateResource(HttpRequestMessage request, ResourceRootSingle resource)
        {
            // Do nothing
        }

        /// <summary>
        /// Modifies requests to update a resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="originalResource">Resource being updated</param>
        /// <param name="patch">Resource patch</param>
        public virtual void UpdateResource(HttpRequestMessage request, Resource originalResource, ResourceRootSingle patch)
        {
            // Do nothing
        }

        /// <summary>
        /// Modifies requests to query resources
        /// </summary>
        /// <param name="request"></param>
        /// <param name="query">Query Context</param>
        public virtual void QueryResources(HttpRequestMessage request, IQueryContext query)
        {
            // Do nothing
        }

        public virtual void DeleteResource(HttpRequestMessage request, Guid id, string resourceType)
        {
            // Do nothing
        }
    }
}
