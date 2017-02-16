using System;
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
            var relatedResources = new Dictionary<string, IEnumerable<ResourceIdentifier>>();
            var resourceIdentifiers = new List<ResourceIdentifier>();

            Task.WaitAll(
                HandleHasSingleConfiguration(modelType, model, resourceIdentifiers),
                HandleHasManyConfigurations(modelType, model, resourceIdentifiers)
            );

            foreach (var resourceIdentifier in resourceIdentifiers)
            {

                var linkName = ModelRegistry.GetModelType(resourceIdentifier.Type).Name.ToLower().Pluralize();

                if (!relatedResources.ContainsKey(linkName)) // included link does not exist
                {
                    relatedResources.Add(linkName, new List<ResourceIdentifier> { resourceIdentifier });
                }
                else
                {
                    var resources = relatedResources[linkName];
                    if (resources.All(x => x.Id != resourceIdentifier.Id))
                    {
                        resources.ToList().Add(resourceIdentifier);
                        relatedResources[linkName] = resources;
                    }
                }
            }

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

        private Task HandleHasManyConfigurations(Type modelType, object model, List<ResourceIdentifier> resourceIdentifiers)
        {
            var objects = ModelRegistry
               .GetCollectionConfigurations(modelType)
               ?.Select(x => x.PropertyInfo.GetValue(model))
               .ToList()
               .Where(x => x != null);

            if (objects == null) return Task.CompletedTask;

            foreach (var obj in objects)
            {
                AssembleResourceRelationships((IEnumerable)obj, resourceIdentifiers);
            }
            return Task.CompletedTask;
        }

        private Task HandleHasSingleConfiguration(Type modelType, object model, List<ResourceIdentifier> resourceIdentifiers)
        {
            var objects = ModelRegistry
               .GetSingleConfigurations(modelType)
               ?.Select(x => x.PropertyInfo.GetValue(model))
               .ToList()
               .Where(x => x != null);

            if (objects == null) return Task.CompletedTask;

            AssembleResourceRelationships(objects, resourceIdentifiers);

            return Task.CompletedTask;
        }

        private void AssembleResourceRelationships(object objects, List<ResourceIdentifier> resourceIdentifiers)
        {
            foreach (var obj in (IEnumerable)objects)
            {
                resourceIdentifiers.Add(new ResourceIdentifier
                {
                    Id = ModelRegistry.GetModelId(obj),
                    Type = ModelRegistry.GetResourceType(obj.GetType())
                });
            }
        }
    }
}
