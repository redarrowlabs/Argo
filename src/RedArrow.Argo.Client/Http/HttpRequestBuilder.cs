using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using RedArrow.Argo.Client.Flurl.Shared;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Query;

namespace RedArrow.Argo.Client.Http
{
    internal class HttpRequestBuilder : IHttpRequestBuilder
    {
        private const string JsonApiHeader = "application/vnd.api+json";

        private JsonSerializerSettings JsonSettings { get; }

        public HttpRequestBuilder(JsonSerializerSettings jsonSettings)
        {
            JsonSettings = jsonSettings;
        }

        public HttpRequestMessage GetResource(Guid id, string resourceType, string include)
        {
            var path = $"{resourceType}/{id}";
            if (!string.IsNullOrEmpty(include))
            {
                path = path.SetQueryParam("include", include);
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
                Content = BuildHttpContent(root.ToJson(JsonSettings)),
            };
        }

        public HttpRequestMessage UpdateResource(Resource patch, IEnumerable<Resource> include)
        {
            var root = ResourceRootSingle.FromResource(patch, include);

            // TODO: investigate - writing via stream may be more performant / smaller memory footprint
            // TODO: since it would avoid loading potentially large strings into memory
            //var stream = new MemoryStream();
            //var sr = new StreamWriter(stream);
            //var writer = new JsonTextWriter(sr);
            //new JsonSerializer().Serialize(writer, root);
            //var content = new StreamContent(stream);

            return new HttpRequestMessage(new HttpMethod("PATCH"), $"{patch.Type}/{patch.Id}")
            {
                Content = BuildHttpContent(root.ToJson(JsonSettings))
            };
        }

        public HttpRequestMessage QueryResources(IQueryContext query, string include)
        {
            var path = query.BuildPath();
            if (!string.IsNullOrEmpty(include))
            {
                path = path.SetQueryParam("include", include);
            }

            return new HttpRequestMessage(HttpMethod.Get, path);
        }

        private static HttpContent BuildHttpContent(string content)
        {
            // TODO: investigate StreamContent as it could offer performance and memory footprint benefits for large objects
            return new StringContent(content, Encoding.UTF8, JsonApiHeader);
        }
    }
}