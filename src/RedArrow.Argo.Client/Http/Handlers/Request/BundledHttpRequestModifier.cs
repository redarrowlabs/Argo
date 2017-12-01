using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Query;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace RedArrow.Argo.Client.Http.Handlers.Request
{
    internal class BundledHttpRequestModifier : HttpRequestModifier
    {
        private IList<HttpRequestModifier> HttpRequestModifiers { get; }
        public BundledHttpRequestModifier(IList<HttpRequestModifier> modifiers)
        {
            HttpRequestModifiers = modifiers;
        }

        public override void GetResource(HttpRequestMessage request, Guid id, string resourceType)
        {
            foreach (var httpRequestModifier in HttpRequestModifiers)
            {
                httpRequestModifier.GetResource(request, id, resourceType);
            }
        }

        public override void GetRelated(HttpRequestMessage request, Guid resourceId, string resourceType, string rltnName)
        {
            foreach (var httpRequestModifier in HttpRequestModifiers)
            {
                httpRequestModifier.GetRelated(request, resourceId, resourceType, rltnName);
            }
        }

        public override void CreateResource(HttpRequestMessage request, ResourceRootSingle resource)
        {
            foreach (var httpRequestModifier in HttpRequestModifiers)
            {
                httpRequestModifier.CreateResource(request, resource);
            }
        }

        public override void UpdateResource(HttpRequestMessage request, Resource originalResource, ResourceRootSingle patch)
        {
            foreach (var httpRequestModifier in HttpRequestModifiers)
            {
                httpRequestModifier.UpdateResource(request, originalResource, patch);
            }
        }

        public override void QueryResources(HttpRequestMessage request, IQueryContext query)
        {
            foreach (var httpRequestModifier in HttpRequestModifiers)
            {
                httpRequestModifier.QueryResources(request, query);
            }
        }

        public override void DeleteResource(HttpRequestMessage request, Guid id, string resourceType)
        {
            foreach (var httpRequestModifier in HttpRequestModifiers)
            {
                httpRequestModifier.DeleteResource(request, id, resourceType);
            }
        }
    }
}
