using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using RedArrow.Jsorm.Config.Model;
using RedArrow.Jsorm.Extensions;

namespace RedArrow.Jsorm.Session.Registry
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

        public Guid GetModelId<TModel>(TModel model)
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

        public HttpRequestMessage CreateGetRequest<TModel>(Guid id)
        {
            return CreateGetRequest(typeof(TModel), id);
        }

        public HttpRequestMessage CreateGetRequest(Type modelType, Guid id)
        {
            return GetModelConfig(modelType).CreateGetRequest(id);
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
            throw new Exception("model not registered");
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