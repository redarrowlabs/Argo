using System;
using RedArrow.Jsorm.Client.Session.Patch;

namespace RedArrow.Jsorm.Client.Http
{
    internal interface IHttpRequestBuilder
    {
        RequestContext GetResource(Guid id, Type modelType);
        RequestContext CreateResource(Type modelType, object model);
        RequestContext UpdateResource(Guid id, object model, PatchContext patchContext);
    }
}
