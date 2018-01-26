using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Exceptions;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Session;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RedArrow.Argo.Client.Collections;
using RedArrow.Argo.Client.Collections.Generic;

namespace RedArrow.Argo.Client.Session.Registry
{
    internal class ModelRegistry : IModelRegistry
    {
        private IDictionary<Type, ModelConfiguration> Registry { get; }
        private IDictionary<string, Type> ResourceTypeToModelType { get; }
        private JsonSerializerSettings JsonSettings { get; }

        internal ModelRegistry(IEnumerable<ModelConfiguration> config, JsonSerializerSettings jsonSettings)
        {
            Registry = config.ToDictionary(x => x.ModelType, x => x);
            ResourceTypeToModelType = Registry.ToDictionary(
                kvp => kvp.Value.ResourceType,
                kvp => kvp.Key);
            JsonSettings = jsonSettings;
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
            return ResourceTypeToModelType.TryGetValue(resourceType, out Type ret)
                ? ret
                : null;
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

        public void ApplyPatch(object model, Resource patch)
        {
            GetResource(model).Patch(patch);
        }

        #endregion Resource

        public bool IsManagedModel(object model)
        {
            var modelType = model.GetType();
            return (bool)GetModelConfig(modelType).SessionManagedProperty.GetValue(model);
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
            return (string)GetModelConfig<TModel>().IncludeField.GetValue(null);
        }

        public Guid GetId(object model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
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
            if (modelId != Guid.Empty) return modelId;
            modelId = Guid.NewGuid();
            SetId(model, modelId);
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
            if (!GetModelConfig(modelType).AttributeConfigs.TryGetValue(attrName, out var ret))
            {
                throw new AttributeNotRegisteredException(attrName, modelType);
            }
            return ret;
        }

        public IEnumerable<MetaConfiguration> GetMetaConfigs<TModel>()
        {
            return GetMetaConfigs(typeof(TModel));
        }

        public IEnumerable<MetaConfiguration> GetMetaConfigs(Type modelType)
        {
            return GetModelConfig(modelType).MetaConfigs.Values;
        }

        public MetaConfiguration GetMetaConfig(Type modelType, string attrName)
        {
            if (!GetModelConfig(modelType).MetaConfigs.TryGetValue(attrName, out var ret))
            {
                throw new MetaNotRegisteredException(attrName, modelType);
            }
            return ret;
        }

        public TAttr GetAttributeValue<TModel, TAttr>(TModel model, string attrName)
        {
            var attributes = GetResource(model).Attributes;
            if (attributes == null)
            {
                // no attributes are present
                return default(TAttr);
            }

            var at = attributes.SelectToken(attrName);
            if (at == null)
            {
                // attribute does not exist
                return default(TAttr);
            }

            // if we make it here, 'attr' has been set
            return at.ToObject<TAttr>(JsonSerializer.CreateDefault(JsonSettings));
        }

        public JObject GetAttributeValues(object model)
        {
            if (model == null) return new JObject();
            // Include null values so we can build the patch correctly
            var attrValues = GetAttributeConfigs(model.GetType())
                .Select(x => new KeyValuePair<string, object>(x.AttributeName, x.Property.GetValue(model)))
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value);

            var result = new JObject();
            foreach (var key in attrValues.Keys)
            {
                BuildObject(result, key, attrValues[key]);
            }

            return result;
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
            if (!GetModelConfig(modelType).HasManyProperties.TryGetValue(rltnName, out var ret))
            {
                throw new RelationshipNotRegisteredExecption(rltnName, modelType);
            }
            return ret;
        }

        public object[] IncludedModelsCreate(object model)
        {
            return model == null
                ? null
                : CreateIncludedModels(model, new[] { model }).ToArray();
        }

        private IEnumerable<object> CreateIncludedModels(object model, object[] parentModels)
        {
            var modelType = model.GetType();
            var relatedModels = GetHasOneConfigs(modelType)
                .Select(hasOne => hasOne.PropertyInfo.GetValue(model))
                .Union(GetHasManyConfigs(modelType)
                    .Select(hasMany => hasMany.PropertyInfo.GetValue(model))
                    .Where(x => !(x is IRemoteCollection) || ((IRemoteCollection) x).IsModified)
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
            var resource = GetResource(model);

            var ret = new Dictionary<string, Relationship>();

            foreach (var hasOne in GetHasOneConfigs(modelType))
            {
                ret[hasOne.RelationshipName] = new Relationship
                {
                    Data = JValue.CreateNull()
                };
                var related = hasOne.PropertyInfo.GetValue(model);
                if (related != null)
                {
                    ret[hasOne.RelationshipName].Data =
                        JObject.FromObject(new ResourceIdentifier
                        {
                            Id = GetOrCreateId(related),
                            Type = GetResourceType(related.GetType())
                        });
                }
            }

            foreach (var hasMany in GetHasManyConfigs(modelType))
            {
                var collection = hasMany.PropertyInfo.GetValue(model) as IEnumerable;
                ret[hasMany.RelationshipName] = new Relationship
                {
                    Data = JValue.CreateNull()
                };
                // Try not to fetch the existing data if not needed
                if (collection is IRemoteCollection managedCollection && !managedCollection.IsModified)
                {
                    if (resource.GetRelationships()
                        .TryGetValue(hasMany.RelationshipName, out var existingRtln))
                    {
                        ret[hasMany.RelationshipName].Data = existingRtln.Data.DeepClone();
                    }
                }
                else if(collection != null)
                {
                    ret[hasMany.RelationshipName].Data = JArray.FromObject(collection
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
                            .ToArray());
                }
                
            }

            return ret;
        }

        public TMeta GetMetaValue<TModel, TMeta>(TModel model, string metaName)
        {
            var meta = GetResource(model).Meta;
            if (meta == null)
            {
                // no meta are present
                return default(TMeta);
            }

            var mt = meta.SelectToken(metaName);
            if (mt == null)
            {
                return default(TMeta);
            }
            return mt.ToObject<TMeta>(JsonSerializer.CreateDefault(JsonSettings));
        }

        public JObject GetMetaValues(object model)
        {
            if (model == null) return new JObject();
            var metaValues = GetMetaConfigs(model.GetType())
                .Select(x => new KeyValuePair<string, object>(x.MetaName, x.Property.GetValue(model)))
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value);

            var result = new JObject();
            foreach (var key in metaValues.Keys)
            {
                BuildObject(result, key, metaValues[key]);
            }

            return result;
        }

        private ModelConfiguration GetModelConfig<TModel>()
        {
            return GetModelConfig(typeof(TModel));
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

        private void BuildObject(JToken token, string name, object value)
        {
            var pathSegments = name.Split(new[] { '.' }, 2);
            if (pathSegments.Length > 1)
            {
                var obj = new JObject();
                BuildObject(obj, pathSegments[1], value);
                token[pathSegments[0]] = obj;
            }
            else
            {
                token[name] = value == null
                    ? JValue.CreateNull()
                    : JToken.FromObject(value, JsonSerializer.CreateDefault(JsonSettings));
            }
        }
    }
}
