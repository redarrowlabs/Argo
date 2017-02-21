using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Flurl.Shared;
using RedArrow.Argo.Client.JsonModels;

namespace RedArrow.Argo.Client.Services.Includes
{
    public interface IIncludeResourcesService
    {
        Task<IEnumerable<Resource>> Process(Type modelType, object model, IDictionary<Guid, Resource> resourceState);
        Task<Url> BuildIncludesUrl(Type modelType, string url);
    }
}
