using System;
using System.Collections.Generic;
using System.Net.Http;
using RedArrow.Argo.Client.Config.Model;

namespace RedArrow.Argo.Client.Session.Registry
{
    public interface IModelRegistry
    {
        string GetResourceType<TModel>();

        string GetResourceType(Type modelType);

        Guid GetModelId<TModel>(TModel model);

        IEnumerable<AttributeConfiguration> GetModelAttributes<TModel>();

        IEnumerable<AttributeConfiguration> GetModelAttributes(Type modelType);

        HttpRequestMessage CreateGetRequest<TModel>(Guid id);

        HttpRequestMessage CreateGetRequest(Type modelType, Guid id);
    }
}