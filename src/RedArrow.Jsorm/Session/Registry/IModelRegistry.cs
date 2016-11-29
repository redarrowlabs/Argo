using System;
using System.Collections.Generic;
using System.Net.Http;
using RedArrow.Jsorm.Config.Model;

namespace RedArrow.Jsorm.Session.Registry
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