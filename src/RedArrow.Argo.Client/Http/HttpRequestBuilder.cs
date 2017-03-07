﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using RedArrow.Argo.Client.Flurl.Shared;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Query;

namespace RedArrow.Argo.Client.Http
{
    internal class HttpRequestBuilder : IHttpRequestBuilder
    {
        private const string JsonApiHeader = "application/vnd.api+json";
		
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
                Content = BuildHttpContent(root.ToJson()),
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
                Content = BuildHttpContent(root.ToJson())
            };
        }

        public HttpRequestMessage GetResources(string resourceType, QueryContext query, string include)
        {
            var path = resourceType;
            if (!string.IsNullOrEmpty(include))
            {
                path = path.SetQueryParam("include", include);
            }

            if (!string.IsNullOrEmpty(query?.Filter))
            {
                path = path.SetQueryParam("filter", query.Filter);
            }

            if (!string.IsNullOrEmpty(query?.Sort))
            {
                path = path.SetQueryParam("sort", query.Sort);
                
                if (query.PageSize != null)
                {
                    path = path.SetQueryParam("page[size]", query.PageSize);
                }
                if (query.PageNumber != null)
                {
                    path = path.SetQueryParam("page[number]", query.PageNumber);
                }
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
