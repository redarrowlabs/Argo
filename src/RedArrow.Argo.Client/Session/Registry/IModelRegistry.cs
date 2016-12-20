using System;
using System.Collections.Generic;
using RedArrow.Argo.Client.Config.Model;

namespace RedArrow.Argo.Client.Session.Registry
{
    public interface IModelRegistry
    {
        string GetResourceType<TModel>();

        string GetResourceType(Type modelType);

        Guid GetModelId(object model);

        IEnumerable<AttributeConfiguration> GetModelAttributes<TModel>();

        IEnumerable<AttributeConfiguration> GetModelAttributes(Type modelType);

        HasManyConfiguration GetCollectionConfiguration<TModel>(string rltnName);

        HasManyConfiguration GetCollectionConfiguration(Type modelType, string rltnName);
    }
}