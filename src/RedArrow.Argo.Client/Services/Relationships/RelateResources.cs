using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Session.Registry;

namespace RedArrow.Argo.Client.Services.Relationships
{
    public class RelateResources : IRelateResources
    {
        private IModelRegistry ModelRegistry { get; }

        public RelateResources(IModelRegistry modelRegistry)
        {
            ModelRegistry = modelRegistry;
        }

        public IDictionary<string, Relationship> Process(IDictionary<string, IEnumerable<Resource>> included)
        {
            var relationships = new Dictionary<string, Relationship>();

            foreach (var include in included)
            {
                var includes = new List<ResourceIdentifier>();
                foreach (var thing in include.Value)
                {
                    includes.Add(thing.ToResourceIdentifier());
                }

                var relationship = new Relationship
                {
                    Data = JToken.FromObject(includes)
                };

                relationships.Add(include.Key, relationship);
            }

            return relationships;
        }

        public IDictionary<string, Relationship> Process(Type modelType, object model)
        {
            var relationships = new Dictionary<string, Relationship>();
            var relatedResources = new Dictionary<string, ICollection<ResourceIdentifier>>();

            CreateRelationshipsMap(modelType, model, relationships);

            HandleHasSingleConfiguration(modelType, model, relatedResources);
            HandleHasManyConfigurations(modelType, model, relatedResources);

            foreach (var relatedResource in relatedResources)
            {
                Relationship relationship = new Relationship();
                var singleConfigurations = ModelRegistry.GetHasOneConfigs(modelType)
                    .Select(x => x.RelationshipType)
                    .ToList();

                foreach (var resourceIdentifier in relatedResource.Value)
                {
                    var resourceType = ModelRegistry.GetModelType(resourceIdentifier.Type);
                    if (singleConfigurations.Contains(resourceType))
                    {
                        relationship = new Relationship
                        {
                            Data = JToken.FromObject(resourceIdentifier)
                        };
                    }
                    else
                    {
                        relationship = new Relationship
                        {
                            Data = JToken.FromObject(relatedResource.Value)
                        };
                    }
                }

                relationships[relatedResource.Key] = relationship;
            }

            if (relationships.Any())
            {
                return relationships;
            }
            return null;
        }

        private void CreateRelationshipsMap(Type modelType, object model, Dictionary<string, Relationship> relationships)
        {
            if (relationships == null)
            {
                relationships = new Dictionary<string, Relationship>();
            }

            var configurations = ModelRegistry
                .GetHasOneConfigs(modelType)
                ?.Select(x => x.RelationshipName)
                .Concat(ModelRegistry
                .GetHasManyConfigs(modelType)
                ?.Select(x => x.RelationshipName));

            foreach (var configuration in configurations)
            {
                if (!relationships.ContainsKey(configuration))
                {
                    relationships[configuration] = new Relationship();
                }
            }
        }

        private void HandleHasManyConfigurations(Type modelType, object model, IDictionary<string, ICollection<ResourceIdentifier>> resourceIdentifiers)
        {
            ModelRegistry
                .GetHasManyConfigs(modelType)
                ?.Each(x =>
                {
                    var value = x.PropertyInfo.GetValue(model);
                    if (value != null)
                    {
                        var rltnName = x.RelationshipName;
                        if (value is IEnumerable)
                        {
                            foreach (var val in (IEnumerable)value)
                            {
                                AssembleResourceRelationships(val, rltnName, resourceIdentifiers);
                            }
                        }
                        else
                        {
                            AssembleResourceRelationships(value, rltnName, resourceIdentifiers);
                        }
                    }
                });
        }

        private void HandleHasSingleConfiguration(Type modelType, object model, IDictionary<string, ICollection<ResourceIdentifier>> resourceIdentifiers)
        {
            ModelRegistry
                .GetHasOneConfigs(modelType)
                ?.Each(x =>
                {
                    var value = x.PropertyInfo.GetValue(model);
                    if (value != null)
                    {
                        var rltnName = x.RelationshipName;
                        if (value is IEnumerable)
                        {
                            foreach (var val in (IEnumerable)value)
                            {
                                AssembleResourceRelationships(val, rltnName, resourceIdentifiers);
                            }
                        }
                        else
                        {
                            AssembleResourceRelationships(value, rltnName, resourceIdentifiers);
                        }
                    }
                });
        }

        private void AssembleResourceRelationships(object model, string rltnName,
            IDictionary<string, ICollection<ResourceIdentifier>> resourceIdentifiers)
        {
            ICollection<ResourceIdentifier> resourceIds;
            if (!resourceIdentifiers.TryGetValue(rltnName, out resourceIds))
            {
                resourceIds = new List<ResourceIdentifier>();
                resourceIdentifiers[rltnName] = resourceIds;
            }

            resourceIds.Add(new ResourceIdentifier
            {
                Id = ModelRegistry.GetId(model),
                Type = ModelRegistry.GetResourceType(model.GetType())
            });
        }
    }
}
