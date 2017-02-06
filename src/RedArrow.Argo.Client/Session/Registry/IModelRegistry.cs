using System;
using System.Collections.Generic;
using RedArrow.Argo.Client.Config.Model;

namespace RedArrow.Argo.Client.Session.Registry
{
    public interface IModelRegistry
    {
        string GetResourceType<TModel>();

        string GetResourceType(Type modelType);

        Type GetModelType(string resourceType);

        Guid GetModelId(object model);

        void SetModelId(object model, Guid id);

        IEnumerable<AttributeConfiguration> GetModelAttributes<TModel>();

        IEnumerable<AttributeConfiguration> GetModelAttributes(Type modelType);

        IEnumerable<HasOneConfiguration> GetSingleConfigurations<TModel>();

        IEnumerable<HasOneConfiguration> GetSingleConfigurations(Type modelType);

        IEnumerable<HasManyConfiguration> GetCollectionConfigurations<TModel>();

        IEnumerable<HasManyConfiguration> GetCollectionConfigurations(Type modelType);

        HasManyConfiguration GetCollectionConfiguration<TModel>(string rltnName);

        HasManyConfiguration GetCollectionConfiguration(Type modelType, string rltnName);
    }
}