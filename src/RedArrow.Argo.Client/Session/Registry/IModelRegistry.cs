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
        Resource GetPatch(object model);
        void SetPatch(object model, Resource patch);
	    Resource GetOrCreatePatch(object model);
	    void ApplyPatch(object model);

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

        JObject GetAttributeValues(object model);
	    IDictionary<string, Relationship> GetRelationshipValues(object model);
        IDictionary<string, JToken> GetMetaValues(object model);

        IEnumerable<RelationshipConfiguration> GetHasOneConfigs<TModel>();
		IEnumerable<RelationshipConfiguration> GetHasOneConfigs(Type modelType);
        IEnumerable<RelationshipConfiguration> GetHasManyConfigs<TModel>();
        IEnumerable<RelationshipConfiguration> GetHasManyConfigs(Type modelType);
        RelationshipConfiguration GetHasManyConfig<TModel>(string rltnName);
        RelationshipConfiguration GetHasManyConfig(Type modelType, string rltnName);

        JObject GetUnmappedAttributes(object model);
        void SetUnmappedAttributes(object model, JObject attributes);

        object[] IncludedModelsCreate(object model);
    }
}