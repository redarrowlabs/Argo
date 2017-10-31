using RedArrow.Argo.Client.Http.Handlers.Request;
using RedArrow.Argo.Client.Model;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RedArrow.Argo.TestUtils
{
    public class EtagRequestModifier : HttpRequestModifier
    {
        public override void UpdateResource(HttpRequestMessage request, Resource resource, ResourceRootSingle patch)
        {
            resource.GetMeta().TryGetValue("system", out var system);
            var eTag = system?.Value<string>("eTag");
            if (eTag != null)
            {
                request.Headers.IfMatch.Add(new EntityTagHeaderValue(eTag));
            }
        }

        public override void DeleteResource(HttpRequestMessage request, Guid id, string resourceType)
        {
            // Delete anything
            request.Headers.IfMatch.Add(EntityTagHeaderValue.Any);
        }
    }
}
