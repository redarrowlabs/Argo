using System;
using System.Collections.Generic;
using System.Net.Http;
using RedArrow.Argo.Client.Model;

namespace RedArrow.Argo.Client.Http
{
    internal interface IHttpRequestBuilder
    {
        RequestContext GetResource(Guid id, Type modelType);
        RequestContext GetRelated(object owner, string rltnName);
        HttpRequestMessage CreateResource(Resource resource, IEnumerable<Resource> included);
        RequestContext UpdateResource(Guid id, object model);
    }
}
