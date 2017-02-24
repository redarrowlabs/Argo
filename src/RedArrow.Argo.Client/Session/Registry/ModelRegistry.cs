using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Exceptions;

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

        public IEnumerable<AttributeConfiguration> GetAttributeConfigs<TModel>()
        {
            return GetAttributeConfigs(typeof(TModel));
        }

        public IEnumerable<AttributeConfiguration> GetAttributeConfigs(Type modelType)
        {
            return GetModelConfig(modelType).AttributeProperties.Values;
        }

        public JObject GetAttributeValues(object model)
        {
            if (model == null) return null;
            return new JObject(GetAttributeConfigs(model.GetType())
                .ToDictionary(
                    x => x.AttributeName,
                    x => x.Property.GetValue(model)));
        }

        public IEnumerable<HasOneConfiguration> GetHasOneConfigs<TModel>()
        {
            return GetHasOneConfigs(typeof(TModel));
        }

        public IEnumerable<HasOneConfiguration> GetHasOneConfigs(Type modelType)
        {
            return GetModelConfig(modelType).HasOneProperties.Values;
        }

        public IEnumerable<HasManyConfiguration> GetHasManyConfigs<TModel>()
        {
            return GetHasManyConfigs(typeof(TModel));
        }

        public IEnumerable<HasManyConfiguration> GetHasManyConfigs(Type modelType)
        {
            return GetModelConfig(modelType).HasManyProperties.Values;
        }

        public HasManyConfiguration GetHasManyConfig<TModel>(string rltnName)
        {
            return GetHasManyConfig(typeof(TModel), rltnName);
        }

        public HasManyConfiguration GetHasManyConfig(Type modelType, string rltnName)
        {
            HasManyConfiguration ret;
            if (!GetModelConfig(modelType).HasManyProperties.TryGetValue(rltnName, out ret))
            {
                throw new RelationshipNotRegisteredExecption(rltnName, modelType);
            }

            return ret;
        }

        public JObject GetAttributeBag(object model)
        {
            var modelType = model.GetType();
            var attributeBag = GetModelConfig(modelType).AttributeBagProperty?.GetValue(model);

            return attributeBag != null
                ? JObject.FromObject(attributeBag)
                : null;
        }

        public void SetAttributeBag(object model, JObject attributes)
        {
            var modelType = model.GetType();
            var attrBagProp = GetModelConfig(modelType).AttributeBagProperty;

            if (attrBagProp == null) return;

            var mappedAttrNames = GetAttributeConfigs(modelType).Select(x => x.AttributeName);
            var unmappedAttrs = (attributes ?? new JObject()).Properties().Where(x => !mappedAttrNames.Contains(x.Name));
            var jAttrBag = new JObject(unmappedAttrs);
            attrBagProp.SetValue(model, jAttrBag.ToObject(attrBagProp.PropertyType));
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

        public IEnumerable<object> GetIncludedModels(object model)
        {
            return model == null ? null : GetIncludedModels(model, new [] {model});
        }

        private IEnumerable<object> GetIncludedModels(object model, IEnumerable<object> parentModels)
        {
            var modelType = model.GetType();
            var relatedModels = GetHasOneConfigs(modelType)
                .Select(hasOne => hasOne.PropertyInfo.GetValue(model))
                .Where(item => item != null)
                .Union(GetHasManyConfigs(modelType)
                    .Select(hasMany => hasMany.PropertyInfo.GetValue(model))
                    .OfType<IEnumerable>()
                    .SelectMany(collection => collection.Cast<object>())
                    .Where(item => item != null))
                .ToArray();

            var includedModels = parentModels.Union(relatedModels).ToArray();

            return includedModels.Union(relatedModels
                .Where(x => !parentModels.Contains(x))
                .SelectMany(x => GetIncludedModels(x, includedModels)));
        }
    }
}