using System;
using System.Collections.Generic;
using System.Net.Http;
using RedArrow.Argo.Client.Model;

namespace RedArrow.Argo.Client.Http
{
    internal interface IHttpRequestBuilder
    {
        HttpRequestMessage GetResource(Guid id, string resourceType, IEnumerable<string> included);
		HttpRequestMessage GetRelated(Guid resourceId, string resourceType, string rltnName);
        HttpRequestMessage CreateResource(Resource resource, IEnumerable<Resource> included);
        HttpRequestMessage UpdateResource(Resource patch, IEnumerable<Resource> included);
    }
}
