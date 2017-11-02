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
        private const string EtagMetaPath = "system.eTag";
        public override void UpdateResource(HttpRequestMessage request, Resource originalResource, ResourceRootSingle patch)
        {
            var eTag = originalResource.GetMeta().SelectToken(EtagMetaPath)
                ?? originalResource.GetMeta().SelectToken(EtagMetaPath);
            if (eTag != null)
            {
                request.Headers.IfMatch.Add(new EntityTagHeaderValue(eTag.ToString()));
            }
        }

        public override void DeleteResource(HttpRequestMessage request, Guid id, string resourceType)
        {
            // Delete anything
            request.Headers.IfMatch.Add(EntityTagHeaderValue.Any);
        }
    }
}
