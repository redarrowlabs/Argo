using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Exceptions;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Session;

namespace RedArrow.Argo.Client.Session.Registry
{
    internal class ModelRegistry : IModelRegistry
    {
        private IDictionary<Type, ModelConfiguration> Registry { get; }
        private IDictionary<string, Type> ResourceTypeToModelType { get; }

        internal ModelRegistry(IEnumerable<ModelConfiguration> config)
        {
            Registry = config.ToDictionary(x => x.ModelType, x => x);
            ResourceTypeToModelType = Registry.ToDictionary(
                kvp => kvp.Value.ResourceType,
                kvp => kvp.Key);
        }

        public string GetResourceType<TModel>()
        {
            return GetResourceType(typeof(TModel));
        }

        public string GetResourceType(Type modelType)
        {
            return GetModelConfig(modelType).ResourceType;
        }

        public Type GetModelType(string resourceType)
        {
            Type ret;
            if (ResourceTypeToModelType.TryGetValue(resourceType, out ret))
            {
                return ret;
            }
            return null;
        }

        #region Resource

        public Resource GetResource(object model)
        {
            var modelType = model.GetType();
            return (Resource)GetModelConfig(modelType).ResourceProperty.GetValue(model);
        }
        public void SetResource(object model, Resource resource)
        {
            var modelType = model.GetType();
            GetModelConfig(modelType).ResourceProperty.SetValue(model, resource);
        }

        public Resource GetPatch(object model)
        {
            var modelType = model.GetType();
            return (Resource)GetModelConfig(modelType).PatchProperty.GetValue(model);
        }

        public void SetPatch(object model, Resource resource)
        {
            var modelType = model.GetType();
            GetModelConfig(modelType).PatchProperty.SetValue(model, resource);
        }

        public Resource GetOrCreatePatch(object model)
        {
            var patch = GetPatch(model);
            if (patch == null)
            {
                patch = new Resource {Id = GetId(model), Type = GetResourceType(model.GetType())};
                SetPatch(model, patch);
            }
            return patch;
        }

	    public void ApplyPatch(object model)
		{
			GetResource(model).Patch(GetPatch(model));
			SetPatch(model, null);
	    }

        #endregion

        public bool IsManagedModel(object model)
	    {
		    var modelType = model.GetType();
		    return (bool) GetModelConfig(modelType).SessionManagedProperty.GetValue(model);
		}

	    public bool IsManagedBy(IModelSession session, object model)
	    {
		    var modelType = model.GetType();
		    return session == GetModelConfig(modelType).SessionField.GetValue(model);
	    }

		public bool IsUnmanagedModel(object model)
		{
			return !IsManagedModel(model);
		}

	    public void DetachModel(object model)
	    {
		    var modelType = model.GetType();
			GetModelConfig(modelType).SessionField.SetValue(model, null);
	    }

	    public string GetInclude<TModel>()
	    {
		    return (string) GetModelConfig<TModel>().IncludeField.GetValue(null);
	    }

		public Guid GetId(object model)
        {
            var modelType = model.GetType();
            return (Guid)GetModelConfig(modelType).IdProperty.GetValue(model);
        }

        public void SetId(object model, Guid id)
        {
            var modelType = model.GetType();
            GetModelConfig(modelType).IdProperty.SetValue(model, id);
        }

        public Guid GetOrCreateId(object model)
        {
            var modelId = GetId(model);
            if (modelId == Guid.Empty)
            {
                modelId = Guid.NewGuid();
                SetId(model, modelId);
            }
            return modelId;
        }

        public IEnumerable<AttributeConfiguration> GetAttributeConfigs<TModel>()
        {
            return GetAttributeConfigs(typeof(TModel));
        }

        public IEnumerable<AttributeConfiguration> GetAttributeConfigs(Type modelType)
        {
            return GetModelConfig(modelType).AttributeConfigs.Values;
        }

        public AttributeConfiguration GetAttributeConfig(Type modelType, string attrName)
        {
            AttributeConfiguration ret;
            if (!GetModelConfig(modelType).AttributeConfigs.TryGetValue(attrName, out ret))
            {
                throw new AttributeNotRegisteredException(attrName, modelType);
            }

            return ret;
        }

        public JObject GetAttributeValues(object model)
        {
            if (model == null) return null;
	        var attrValues = GetAttributeConfigs(model.GetType())
		        .Select(x => new KeyValuePair<string, object>(x.AttributeName, x.Property.GetValue(model)))
		        .Where(kvp => kvp.Value != null)
		        .ToDictionary(
			        kvp => kvp.Key,
			        kvp => kvp.Value);
	        return attrValues.Any()
		        ? JObject.FromObject(attrValues)
		        : null;
        }

        public IEnumerable<RelationshipConfiguration> GetHasOneConfigs<TModel>()
        {
            return GetHasOneConfigs(typeof(TModel));
        }

        public IEnumerable<RelationshipConfiguration> GetHasOneConfigs(Type modelType)
        {
            return GetModelConfig(modelType).HasOneProperties.Values;
        }

        public IEnumerable<RelationshipConfiguration> GetHasManyConfigs<TModel>()
        {
            return GetHasManyConfigs(typeof(TModel));
        }

        public IEnumerable<RelationshipConfiguration> GetHasManyConfigs(Type modelType)
        {
            return GetModelConfig(modelType).HasManyProperties.Values;
        }

        public RelationshipConfiguration GetHasManyConfig<TModel>(string rltnName)
        {
            return GetHasManyConfig(typeof(TModel), rltnName);
        }

        public RelationshipConfiguration GetHasManyConfig(Type modelType, string rltnName)
        {
			RelationshipConfiguration ret;
            if (!GetModelConfig(modelType).HasManyProperties.TryGetValue(rltnName, out ret))
            {
                throw new RelationshipNotRegisteredExecption(rltnName, modelType);
            }

            return ret;
        }

        public JObject GetUnmappedAttributes(object model)
        {
            var modelType = model.GetType();
            var unmapped = GetModelConfig(modelType).UnmappedAttributesProperty?.GetValue(model);

            return unmapped != null
                ? JObject.FromObject(unmapped)
                : null;
        }

        public void SetUnmappedAttributes(object model, JObject attributes)
        {
            var modelType = model.GetType();
            var unmappedProp = GetModelConfig(modelType).UnmappedAttributesProperty;

            if (unmappedProp == null) return;

            var mappedAttrNames = GetAttributeConfigs(modelType).Select(x => x.AttributeName);
            var unmappedAttrs = attributes?.Properties().Where(x => !mappedAttrNames.Contains(x.Name));
            if (unmappedAttrs == null) return;
            unmappedProp.SetValue(model, new JObject(unmappedAttrs).ToObject(unmappedProp.PropertyType));
        }

        private ModelConfiguration GetModelConfig<TModel>()
	    {
		    return GetModelConfig(typeof (TModel));
	    }

        private ModelConfiguration GetModelConfig(Type modelType)
        {
            ThrowIfNotRegistered(modelType);
            return Registry[modelType];
        }

        private void ThrowIfNotRegistered(Type type)
        {
            if (!Registry.ContainsKey(type))
            {
                throw new ModelNotRegisteredException(type);
            }
        }

        public object[] IncludedModelsCreate(object model)
        {
            return model == null ? null : CreateIncludedModels(model, new [] {model}).ToArray();
        }

        private IEnumerable<object> CreateIncludedModels(object model, object[] parentModels)
        {
			var modelType = model.GetType();
			var relatedModels = GetHasOneConfigs(modelType)
				.Select(hasOne => hasOne.PropertyInfo.GetValue(model))
				.Union(GetHasManyConfigs(modelType)
					.Select(hasMany => hasMany.PropertyInfo.GetValue(model))
					.OfType<IEnumerable>()
					.SelectMany(collection => collection.Cast<object>()))
                .Where(x => x != null)
				.ToArray();

			var includedModels = parentModels.Union(relatedModels).ToArray();

	        return includedModels.Union(relatedModels
		        .Where(x => !parentModels.Contains(x))
		        .SelectMany(x => CreateIncludedModels(x, includedModels)))
                .Where(IsUnmanagedModel)
                .ToArray();
        }

        public IDictionary<string, Relationship> GetRelationshipValues(object model)
		{
			var modelType = model.GetType();

			var ret = new Dictionary<string, Relationship>();

		    foreach (var hasOne in GetHasOneConfigs(modelType))
		    {
			    var related = hasOne.PropertyInfo.GetValue(model);
			    if (related == null) continue;
		        var id = GetOrCreateId(related);
			    ret[hasOne.RelationshipName] = new Relationship
                {
                    Data = JObject.FromObject(new ResourceIdentifier
                    {
                        Id = id,
                        Type = GetResourceType(related.GetType())
                    })
                };
		    }

		    foreach (var hasMany in GetHasManyConfigs(modelType))
		    {
			    var collection = hasMany.PropertyInfo.GetValue(model) as IEnumerable;
			    if (collection == null) continue;
		        ret[hasMany.RelationshipName] = new Relationship
		        {
		            Data = JArray.FromObject(collection
                        .Cast<object>()
		                .Select(related =>
		                {
		                    var id = GetOrCreateId(related);
		                    return new ResourceIdentifier
		                    {
		                        Id = id,
		                        Type = GetResourceType(related.GetType())
		                    };
		                })
		                .ToArray())
		        };
		    }

		    return ret;
		}
    }
}