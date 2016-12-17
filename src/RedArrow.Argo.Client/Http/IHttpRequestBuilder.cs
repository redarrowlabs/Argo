using System;
using RedArrow.Argo.Client.Session.Patch;

namespace RedArrow.Argo.Client.Http
{
    internal interface IHttpRequestBuilder
    {
        RequestContext CreateResource(Type modelType, object model);
        RequestContext UpdateResource(Guid id, object model, PatchContext patchContext);
    }
}
