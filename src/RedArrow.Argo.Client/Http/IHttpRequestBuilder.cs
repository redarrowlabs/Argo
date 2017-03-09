using System;
using System.Collections.Generic;
using System.Net.Http;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Query;

namespace RedArrow.Argo.Client.Http
{
    public interface IHttpRequestBuilder
    {
        HttpRequestMessage GetResource(Guid id, string resourceType, string include);
		HttpRequestMessage GetRelated(Guid resourceId, string resourceType, string rltnName);
        HttpRequestMessage CreateResource(Resource resource, IEnumerable<Resource> included);
        HttpRequestMessage UpdateResource(Resource patch, IEnumerable<Resource> included);

        HttpRequestMessage GetResources(string resourceType, QueryContext query, string include);
    }
}
