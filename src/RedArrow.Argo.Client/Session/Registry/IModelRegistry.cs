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
        Resource GetPatch(object model);
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

        JObject GetAttributeValues(object model);
	    IDictionary<string, Relationship> GetRelationshipValues(object model);

	    IEnumerable<RelationshipConfiguration> GetHasOneConfigs(Type modelType);
        IEnumerable<RelationshipConfiguration> GetHasManyConfigs<TModel>();
        IEnumerable<RelationshipConfiguration> GetHasManyConfigs(Type modelType);
        RelationshipConfiguration GetHasManyConfig<TModel>(string rltnName);
        RelationshipConfiguration GetHasManyConfig(Type modelType, string rltnName);
		
        object[] IncludedModelsCreate(object model);
    }
}