using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Query;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedArrow.Argo.Client.Http
{
    public interface IHttpRequestBuilder
    {
        HttpRequestMessage GetResource(Guid id, string resourceType, string include);
        HttpRequestMessage GetRelated(Guid resourceId, string resourceType, string rltnName);
        Task<HttpRequestMessage> CreateResource(ResourceRootSingle root);
        Task<HttpRequestMessage> UpdateResource(Resource resource, ResourceRootSingle root);
        HttpRequestMessage DeleteResource(string resourceType, Guid id);

        HttpRequestMessage QueryResources(IQueryContext query, string include);
    }
}