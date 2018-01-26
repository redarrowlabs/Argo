using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Session;

namespace RedArrow.Argo.Client.Session.Registry
{
    public interface IModelRegistry
    {
        string GetResourceType<TModel>();
        string GetResourceType(Type modelType);

        Type GetModelType(string resourceType);

        Resource GetResource(object model);
        void SetResource(object model, Resource resource);
        void ApplyPatch(object model, Resource patch);

        bool IsManagedModel(object model);
        bool IsManagedBy(IModelSession session, object model);
        bool IsUnmanagedModel(object model);
        void DetachModel(object model);

        string GetInclude<TModel>();

        Guid GetId(object model);
        void SetId(object model, Guid id);
        Guid GetOrCreateId(object model);

        IEnumerable<AttributeConfiguration> GetAttributeConfigs<TModel>();
        IEnumerable<AttributeConfiguration> GetAttributeConfigs(Type modelType);
        AttributeConfiguration GetAttributeConfig(Type modelType, string attrName);

        IEnumerable<MetaConfiguration> GetMetaConfigs<TModel>();
        IEnumerable<MetaConfiguration> GetMetaConfigs(Type modelType);
        MetaConfiguration GetMetaConfig(Type modelType, string attrName);

        TAttr GetAttributeValue<TModel, TAttr>(TModel model, string attrName);
        JObject GetAttributeValues(object model);
        IDictionary<string, Relationship> GetRelationshipValues(object model);
        TMeta GetMetaValue<TModel, TMeta>(TModel model, string metaName);
        JObject GetMetaValues(object model);

        IEnumerable<HasOneConfiguration> GetHasOneConfigs<TModel>();
        IEnumerable<HasOneConfiguration> GetHasOneConfigs(Type modelType);
        IEnumerable<HasManyConfiguration> GetHasManyConfigs<TModel>();
        IEnumerable<HasManyConfiguration> GetHasManyConfigs(Type modelType);
        HasManyConfiguration GetHasManyConfig<TModel>(string rltnName);
        HasManyConfiguration GetHasManyConfig(Type modelType, string rltnName);

        object[] IncludedModelsCreate(object model);
    }
}