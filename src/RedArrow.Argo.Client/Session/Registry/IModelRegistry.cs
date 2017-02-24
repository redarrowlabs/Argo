using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Config.Model;

namespace RedArrow.Argo.Client.Session.Registry
{
    public interface IModelRegistry
    {
        string GetResourceType<TModel>();

        string GetResourceType(Type modelType);

        Type GetModelType(string resourceType);


        Guid GetId(object model);

        void SetId(object model, Guid id);


        IEnumerable<AttributeConfiguration> GetAttributeConfigs<TModel>();

        IEnumerable<AttributeConfiguration> GetAttributeConfigs(Type modelType);

        JObject GetAttributeValues(object model);

        IEnumerable<HasOneConfiguration> GetHasOneConfigs(Type modelType);

        IEnumerable<HasManyConfiguration> GetHasManyConfigs<TModel>();

        IEnumerable<HasManyConfiguration> GetHasManyConfigs(Type modelType);

        HasManyConfiguration GetHasManyConfig<TModel>(string rltnName);

        HasManyConfiguration GetHasManyConfig(Type modelType, string rltnName);


        JObject GetAttributeBag(object model);

        void SetAttributeBag(object model, JObject attributes);

        IEnumerable<object> GetIncludedModels(object model);
    }
}