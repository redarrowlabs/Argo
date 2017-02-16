using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Extensions;
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

        public async Task<IDictionary<string, IEnumerable<Resource>>> Process(Type modelType, object model)
        {
            IDictionary<string, IEnumerable<Resource>> included = new Dictionary<string, IEnumerable<Resource>>();
            await AssembleIncluded(modelType, model, included);

            //await RelateResources.Process(modelType, model);

            return included.Any() ? included : null;
        }

        private Task HandleHasManyConfigurations(Type modelType, object model, IDictionary<string, IEnumerable<Resource>> included)
        {
            var objects = ModelRegistry
               .GetCollectionConfigurations(modelType)
               ?.Select(x => x.PropertyInfo.GetValue(model))
               .ToList()
               .Where(x => x != null);

            if (objects == null) return Task.CompletedTask;

            foreach (var obj in objects)
            {
                BuildIncludesTree((IEnumerable)obj, included);
            }
            return Task.CompletedTask;
        }

        private Task HandleHasSingleConfiguration(Type modelType, object model, IDictionary<string, IEnumerable<Resource>> included)
        {
            var objects = ModelRegistry
               .GetSingleConfigurations(modelType)
               ?.Select(x => x.PropertyInfo.GetValue(model))
               .ToList()
               .Where(x => x != null);

            if (objects == null) return Task.CompletedTask;

            BuildIncludesTree(objects, included);

            return Task.CompletedTask;
        }

        private async Task AssembleIncluded(Type modelType, object model, IDictionary<string, IEnumerable<Resource>> included)
        {
            await HandleHasManyConfigurations(modelType, model, included);
            await HandleHasSingleConfiguration(modelType, model, included);
        }

        private void BuildIncludesTree(object objects, IDictionary<string, IEnumerable<Resource>> included)
        {
            var resources = new List<Resource>();
            foreach (var resrc in (IEnumerable)objects)
            {
                var linkName = resrc.GetType().Name.ToLower().Pluralize();

                AssembleIncluded(resrc.GetType(), resrc, included).Wait();

                var resource = AssembleResource(resrc);
                resources.Add(resource);

                if (!included.ContainsKey(linkName)) // included link does not exist
                {
                    included.Add(linkName, resources);
                }
                else
                {
                    var includedResources = included[linkName].ToList();
                    if (includedResources.All(x => x.Id != resource.Id))
                    {
                        includedResources.Add(resource);
                        included[linkName] = includedResources;
                    }
                }
            }
        }

        private Resource AssembleResource(object obj)
        {
            if (!ObjectHash.Contains(obj))
            {
                ObjectHash.Add(obj);
                ModelRegistry.SetModelId(obj, Guid.NewGuid());
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
