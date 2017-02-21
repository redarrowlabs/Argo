using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedArrow.Argo.Client.JsonModels;
using RedArrow.Argo.Client.Session.Patch;

namespace RedArrow.Argo.Client.Http
{
    internal interface IHttpRequestBuilder
    {
        RequestContext GetResource(Guid id, Type modelType);
        RequestContext GetRelated(object owner, string rltnName);
        RequestContext CreateResource(Type modelType, object model, IDictionary<Guid, Resource> resourceState);
        RequestContext UpdateResource(Guid id, object model, PatchContext patchContext, IDictionary<Guid, Resource> resourceState);
    }
}
