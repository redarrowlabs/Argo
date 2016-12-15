using RedArrow.Jsorm.Client.Config.Model;
using System;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Client.Session.Registry
{
    public interface IModelRegistry
    {
        string GetResourceType<TModel>();

        string GetResourceType(Type modelType);

        Guid GetModelId<TModel>(TModel model);

        IEnumerable<AttributeConfiguration> GetModelAttributes<TModel>();

        IEnumerable<AttributeConfiguration> GetModelAttributes(Type modelType);

        HasManyConfiguration GetCollectionConfiguration<TModel>(string rltnName);

        HasManyConfiguration GetCollectionConfiguration(Type modelType, string rltnName);
    }
}