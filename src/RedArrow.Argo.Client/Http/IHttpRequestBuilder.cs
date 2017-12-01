using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Query;

namespace RedArrow.Argo.Client.Http
{
    public interface IHttpRequestBuilder
    {
        HttpRequestMessage GetResource(Guid id, string resourceType, string include);
        HttpRequestMessage GetRelated(Guid resourceId, string resourceType, string rltnName);
        Task<HttpRequestMessage> CreateResource(Resource resource, IEnumerable<Resource> included);
        Task<HttpRequestMessage> UpdateResource(Resource resource, Resource patch, IEnumerable<Resource> included);
        HttpRequestMessage DeleteResource(string resourceType, Guid id);

        HttpRequestMessage QueryResources(IQueryContext query, string include);
    }
}