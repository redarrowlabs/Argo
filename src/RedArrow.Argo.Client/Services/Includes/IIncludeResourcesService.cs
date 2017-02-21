using System;
using System.Collections.Generic;
using RedArrow.Argo.Client.Flurl.Shared;
using RedArrow.Argo.Client.JsonModels;

namespace RedArrow.Argo.Client.Services.Includes
{
    public interface IIncludeResourcesService
    {
        IEnumerable<Resource> Process(Type modelType, object model, IDictionary<Guid, Resource> resourceState);
        Url BuildIncludesUrl(Type modelType, string url);
    }
}
