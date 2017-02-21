using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Flurl.Shared;
using RedArrow.Argo.Client.JsonModels;
using RedArrow.Argo.Client.Services.Relationships;
using RedArrow.Argo.Client.Session.Registry;

namespace RedArrow.Argo.Client.Services.Includes
{
    public class IncludeResourcesService : IIncludeResourcesService
    {
        private IModelRegistry ModelRegistry { get; }

        private IRelateResources RelateResources { get; }

        private HashSet<object> ObjectHash { get; }

        public IncludeResourcesService(IModelRegistry modelRegistry)
        {
            ModelRegistry = modelRegistry;
            RelateResources = new RelateResources(ModelRegistry);

            ObjectHash = new HashSet<object>();
        }

        public Task<Url> BuildIncludesUrl(Type modelType, string url)
        {
            var nodeMap = new List<string>();
            ExtractIncludeType(
                modelType,
                modelType.Name.ToLower(),
                nodeMap);
            return Task.FromResult(url.SetQueryParam($"include", string.Join(",", nodeMap)));
        }

        private void ExtractIncludeType(Type modelType, string currentLevel, List<string> nodeMap = null)
        {
            if (nodeMap == null)
            {
                nodeMap = new List<string>();
            }

            if (currentLevel.Split('.').Length > 1)
            {
                nodeMap.Add(currentLevel);
            }

            var collectionConfiguration = ModelRegistry.GetCollectionConfigurations(modelType)
                .Select(x => x)
                .ToList();
            if (collectionConfiguration != null && collectionConfiguration.Any())
            {
                foreach (var hasManyConfiguration in collectionConfiguration)
                {
                    if (!nodeMap.Select(x => x.Split('.'))
                            .Any(x => x.Contains(hasManyConfiguration.HasManyType.Name.ToLower()))
                            && hasManyConfiguration.Eager)
                    {
                        ExtractIncludeType(
                        hasManyConfiguration.HasManyType,
                        $"{currentLevel}.{hasManyConfiguration.HasManyType.Name.ToLower()}",
                        nodeMap);
                    }
                }
            }

            var singleConfiguration = ModelRegistry.GetSingleConfigurations(modelType)
                .Select(x => x)
                .ToList();
            if (singleConfiguration != null && singleConfiguration.Any())
            {
                foreach (var hasOneConfiguration in singleConfiguration)
                {
                    if (!nodeMap.Select(x => x.Split('.'))
                            .Any(x => x.Contains(hasOneConfiguration.HasOneType.Name.ToLower()))
                            && hasOneConfiguration.Eager)
                    {
                        ExtractIncludeType(
                          hasOneConfiguration.HasOneType,
                          $"{currentLevel}.{hasOneConfiguration.HasOneType.Name.ToLower()}",
                          nodeMap);
                    }
                }
            }
        }

        public async Task<IEnumerable<Resource>> Process(Type modelType, object model, IDictionary<Guid, Resource> resourceState)
        {
            IDictionary<string, ICollection<Resource>> included = new Dictionary<string, ICollection<Resource>>();
            await AssembleIncluded(modelType, model, included, resourceState);

            return included.Any() ? included.SelectMany(x => x.Value).ToList() : null;
        }

        private Task HandleHasManyConfigurations(Type modelType, object model, IDictionary<string, ICollection<Resource>> included, IDictionary<Guid, Resource> resourceState)
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
                                BuildIncludesTree(val, rltnName, included, resourceState);
                            }
                        }
                        else
                        {
                            BuildIncludesTree(value, rltnName, included, resourceState);
                        }
                    }
                });
            return Task.CompletedTask;
        }

        private Task HandleHasSingleConfiguration(Type modelType, object model, IDictionary<string, ICollection<Resource>> included, IDictionary<Guid, Resource> resourceState)
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
                               BuildIncludesTree(val, rltnName, included, resourceState);
                           }
                       }
                       else
                       {
                           BuildIncludesTree(value, rltnName, included, resourceState);
                       }
                   }
               });

            return Task.CompletedTask;
        }

        private async Task AssembleIncluded(Type modelType, object model, IDictionary<string, ICollection<Resource>> included, IDictionary<Guid, Resource> resourceState)
        {
            await HandleHasManyConfigurations(modelType, model, included, resourceState);
            await HandleHasSingleConfiguration(modelType, model, included, resourceState);
        }

        private void BuildIncludesTree(object model, string rltnName, IDictionary<string, ICollection<Resource>> included, IDictionary<Guid, Resource> resourceState)
        {
            ICollection<Resource> resources;
            if (!included.TryGetValue(rltnName, out resources))
            {
                resources = new List<Resource>();
                included[rltnName] = resources;
            }

            AssembleIncluded(model.GetType(), model, included, resourceState).Wait();

            var modelId = ModelRegistry.GetModelId(model);
            if (modelId.Equals(Guid.Empty) && !resourceState.ContainsKey(modelId))
            {
                var resource = AssembleResource(model);

                if (resources.Any(x => x.Id == resource.Id)) return;

                resources.Add(resource);
                included[rltnName] = resources;
            }
        }

        private Resource AssembleResource(object obj)
        {
            if (!ObjectHash.Contains(obj))
            {
                ObjectHash.Add(obj);
                if (ModelRegistry.GetModelId(obj).Equals(Guid.Empty))
                {
                    ModelRegistry.SetModelId(obj, Guid.NewGuid());
                }
            }

            var resource = new Resource
            {
                Attributes = obj != null
                    ? JObject.FromObject(ModelRegistry
                        .GetModelAttributes(obj.GetType())
                        .ToDictionary(
                            x => x.AttributeName,
                            x => x.Property.GetValue(obj)))
                    : null,
                Id = ModelRegistry.GetModelId(obj),
                Type = ModelRegistry.GetResourceType(obj.GetType()),
                Relationships = RelateResources.Process(obj.GetType(), obj)
            };
            resource.Attributes.Remove("Id"); //remove Id from attributes

            return resource;
        }
    }
}
