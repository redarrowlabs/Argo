using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Model;

namespace RedArrow.Argo.Client.Session.Registry
{
    public interface IModelRegistry
    {
        string GetResourceType<TModel>();

        string GetResourceType(Type modelType);

        Type GetModelType(string resourceType);

	    Resource GetResource(object model);

	    bool IsManagedModel(object model);
	    bool IsUnmanagedModel(object model);

        Guid GetId(object model);

        void SetId(object model, Guid id);


        IEnumerable<AttributeConfiguration> GetAttributeConfigs<TModel>();

        IEnumerable<AttributeConfiguration> GetAttributeConfigs(Type modelType);

        JObject GetAttributeValues(object model);
	    IDictionary<string, Relationship> GetRelationshipValues(object model);

	    IEnumerable<RelationshipConfiguration> GetHasOneConfigs(Type modelType);

        IEnumerable<RelationshipConfiguration> GetHasManyConfigs<TModel>();

        IEnumerable<RelationshipConfiguration> GetHasManyConfigs(Type modelType);

        RelationshipConfiguration GetHasManyConfig<TModel>(string rltnName);

        RelationshipConfiguration GetHasManyConfig(Type modelType, string rltnName);


        JObject GetAttributeBag(object model);

        void SetAttributeBag(object model, JObject attributes);

        object[] GetIncludedModels(object model);
    }
}