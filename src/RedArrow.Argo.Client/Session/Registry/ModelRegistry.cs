using System;
using System.Collections.Generic;
using System.Linq;
using RedArrow.Argo.Client.Config.Model;

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

        public Guid GetModelId(object model)
        {
            var modelType = model.GetType();
            return (Guid)GetModelConfig(modelType).IdProperty.GetValue(model);
        }

        public void SetModelId(object model, Guid id)
        {
            var modelType = model.GetType();
            GetModelConfig(modelType).IdProperty.SetValue(model, id);
        }

        public IEnumerable<AttributeConfiguration> GetModelAttributes<TModel>()
        {
            return GetModelAttributes(typeof(TModel));
        }

        public IEnumerable<AttributeConfiguration> GetModelAttributes(Type modelType)
        {
            return GetModelConfig(modelType).AttributeProperties.Values;
        }

        public IEnumerable<HasOneConfiguration> GetSingleConfigurations<TModel>()
        {
            return GetSingleConfigurations(typeof(TModel));
        }

        public IEnumerable<HasOneConfiguration> GetSingleConfigurations(Type modelType)
        {
            return GetModelConfig(modelType).HasOneProperties.Values;
        }

        public IEnumerable<HasManyConfiguration> GetCollectionConfigurations<TModel>()
        {
            return GetCollectionConfigurations(typeof(TModel));
        }

        public IEnumerable<HasManyConfiguration> GetCollectionConfigurations(Type modelType)
        {
            return GetModelConfig(modelType).HasManyProperties.Values;
        }

        public HasManyConfiguration GetCollectionConfiguration<TModel>(string rltnName)
        {
            return GetCollectionConfiguration(typeof(TModel), rltnName);
        }

        public HasManyConfiguration GetCollectionConfiguration(Type modelType, string rltnName)
        {
            HasManyConfiguration ret;
            if (!GetModelConfig(modelType).HasManyProperties.TryGetValue(rltnName, out ret))
            {
                // TODO: RelationNotRegisteredExecption
                throw new Exception($"has-many configuration named {rltnName} not found");
            }
            return ret;
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
                // TODO: ModelNotRegisteredException
                throw new Exception("model not registered");
            }
        }
    }
}