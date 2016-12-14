using System;
using RedArrow.Jsorm.Client.Session.Patch;

namespace RedArrow.Jsorm.Client.Http
{
    internal interface IHttpRequestBuilder
    {
        RequestContext CreateResource(Type modelType, object model);
        RequestContext UpdateResource(Guid id, object model, PatchContext patchContext);
    }
}
