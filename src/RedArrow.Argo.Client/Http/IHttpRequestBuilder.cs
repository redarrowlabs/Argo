using System;
using System.Collections.Generic;
using System.Net.Http;
using RedArrow.Argo.Client.Model;

namespace RedArrow.Argo.Client.Http
{
    internal interface IHttpRequestBuilder
    {
        HttpRequestMessage GetResource(Guid id, string resourceType);
        RequestContext GetRelated(object owner, string rltnName);
        HttpRequestMessage CreateResource(Resource resource, IEnumerable<Resource> included);
        HttpRequestMessage UpdateResource(Resource patch, IEnumerable<Resource> included);
    }
}
