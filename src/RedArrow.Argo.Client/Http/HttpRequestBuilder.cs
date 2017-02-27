using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Flurl.Shared;
using RedArrow.Argo.Client.Model;

namespace RedArrow.Argo.Client.Http
{
    internal class HttpRequestBuilder : IHttpRequestBuilder
    {
        private const string JsonApiHeader = "application/vnd.api+json";
		
        public HttpRequestMessage GetResource(Guid id, string resourceType, IEnumerable<string> included)
        {
            var path = $"{resourceType}/{id}";
            if (!included.IsNullOrEmpty())
            {
                path.SetQueryParam("include", string.Join(",", included));
            }
            return new HttpRequestMessage(HttpMethod.Get, path);
        }

        public HttpRequestMessage GetRelated(Guid id, string resourceType, string rltnName)
        {
			return new HttpRequestMessage(HttpMethod.Get, $"{resourceType}/{id}/{rltnName}");
        }

        public HttpRequestMessage CreateResource(Resource resource, IEnumerable<Resource> include)
		{
			var root = ResourceRootSingle.FromResource(resource, include);

            return new HttpRequestMessage(HttpMethod.Post, resource.Type)
            {
                Content = BuildHttpContent(root.ToJson()),
            };
        }

        public HttpRequestMessage UpdateResource(Resource patch, IEnumerable<Resource> include)
        {
            var root = ResourceRootSingle.FromResource(patch, include);

            return new HttpRequestMessage(new HttpMethod("PATCH"), $"{patch.Type}/{patch.Id}")
            {
                Content = BuildHttpContent(root.ToJson())
            };
        }

        private static HttpContent BuildHttpContent(string content)
        {
			// TODO: investigate StreamContent as it could offer performance and memory footprint benefits for large objects
            return new StringContent(content, Encoding.UTF8, JsonApiHeader);
        }
    }
}
