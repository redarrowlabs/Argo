﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.JsonModels;
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

            HandleHasSingleConfiguration(modelType, model, relatedResources);
            HandleHasManyConfigurations(modelType, model, relatedResources);

            foreach (var relatedResource in relatedResources)
            {
                var relationship = new Relationship
                {
                    Data = JToken.FromObject(relatedResource.Value)
                };

                relationships.Add(relatedResource.Key, relationship);
            }

            if (relationships.Any())
            {
                return relationships;
            }
            return null;
        }

        private void HandleHasManyConfigurations(Type modelType, object model, IDictionary<string, ICollection<ResourceIdentifier>> resourceIdentifiers)
        {
            ModelRegistry
                .GetCollectionConfigurations(modelType)
                ?.ForEach(x =>
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
                .GetSingleConfigurations(modelType)
                ?.ForEach(x =>
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
                Id = ModelRegistry.GetModelId(model),
                Type = ModelRegistry.GetResourceType(model.GetType())
            });
        }
    }
}
