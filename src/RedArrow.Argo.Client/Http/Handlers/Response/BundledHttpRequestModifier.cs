using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RedArrow.Argo.Client.Http.Handlers.Response
{
    /// <summary>
    /// This implementation purposely does not wait for the task to complete
    /// </summary>
    internal class BundledHttpResponseListener
    {
        private IList<HttpResponseListener> HttpResponseListeners { get; }

        public BundledHttpResponseListener()
        {
            HttpResponseListeners = new List<HttpResponseListener>(0);
        }
        public BundledHttpResponseListener(IList<HttpResponseListener> modifiers)
        {
            HttpResponseListeners = modifiers;
        }

        public void GetResource(HttpStatusCode statusCode, Guid id, string resourceType)
        {
            FireAndForget(HttpResponseListeners
                .Select(x => x.GetResource(statusCode, id, resourceType)));
        }

        public void GetRelated(HttpStatusCode statusCode, Guid resourceId, string resourceType, string rltnName)
        {
            FireAndForget(HttpResponseListeners
                .Select(x => x.GetRelated(statusCode, resourceId, resourceType, rltnName)));
        }

        public void CreateResource(HttpStatusCode statusCode, ResourceRootSingle resource)
        {
            FireAndForget(HttpResponseListeners
                .Select(x => x.CreateResource(statusCode, resource)));
        }

        public void UpdateResource(HttpStatusCode statusCode, Resource originalResource, ResourceRootSingle patch)
        {
            FireAndForget(HttpResponseListeners
                .Select(x => x.UpdateResource(statusCode, originalResource, patch)));
        }

        public void QueryResources(HttpStatusCode statusCode, IQueryContext query)
        {
            FireAndForget(HttpResponseListeners
                .Select(x => x.QueryResources(statusCode, query)));
        }

        public void DeleteResource(HttpStatusCode statusCode, Guid id, string resourceType)
        {
            FireAndForget(HttpResponseListeners
                .Select(x => x.DeleteResource(statusCode, id, resourceType)));
        }

        private void FireAndForget(IEnumerable<Task> tasks)
        {
            Task.Run(() => Task.WaitAll(tasks.ToArray()));
        }
    }
}
