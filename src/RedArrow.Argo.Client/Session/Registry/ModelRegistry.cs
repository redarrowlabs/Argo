using System;
using System.Collections.Generic;
using System.Linq;
using RedArrow.Argo.Client.Config.Model;

namespace RedArrow.Argo.Client.Session.Registry
{
    internal class ModelRegistry : IModelRegistry
    {
        private IDictionary<Type, ModelConfiguration> Registry { get; }

        internal ModelRegistry(IEnumerable<ModelConfiguration> config)
        {
            Registry = config.ToDictionary(x => x.ModelType, x => x);
        }

        public string GetResourceType<TModel>()
        {
            return GetResourceType(typeof(TModel));
        }

        public string GetResourceType(Type modelType)
        {
            return GetModelConfig(modelType).ResourceType;
        }

        public Guid GetModelId(object model)
        {
            var modelType = model.GetType();
            return (Guid)GetModelConfig(modelType).IdProperty.GetValue(model);
        }

        public IEnumerable<AttributeConfiguration> GetModelAttributes<TModel>()
        {
            return GetModelAttributes(typeof(TModel));
        }

        public IEnumerable<AttributeConfiguration> GetModelAttributes(Type modelType)
        {
            return GetModelConfig(modelType).AttributeProperties.Values;
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
                throw new Exception($"JSORM||has-many configuration named {rltnName} not found");
            }
            return ret;
        }

        private ModelConfiguration GetModelConfig(Type modelType)
        {
            ThrowIfNotRegistered(modelType);
            ModelConfiguration config;
            if (Registry.TryGetValue(modelType, out config))
            {
                return config;
            }

            // TODO: ModelNotRegisteredException
            throw new Exception("JSORM||model not registered");
        }

        private void ThrowIfNotRegistered(Type type)
        {
            if (!Registry.ContainsKey(type))
            {
                // TODO: ModelNotRegisteredException
                throw new Exception("JSORM||model not registered");
            }
        }
    }
}