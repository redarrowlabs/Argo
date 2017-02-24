using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Model;

namespace RedArrow.Argo.Client.Session.Registry
{
    public class ResourceRegistry : IResourceRegistry
    {
        private IModelRegistry ModelRegistry { get; }

        private IDictionary<Guid, Resource> StagingArea { get; }

        private IDictionary<Guid, Resource> SessionState { get; }

        public ResourceRegistry(IModelRegistry modelRegistry)
        {
            ModelRegistry = modelRegistry;

            StagingArea = new Dictionary<Guid, Resource>();
            SessionState = new Dictionary<Guid, Resource>();
        }
        
        public Guid StageNewResource(Type modelType, object model)
        {
            var resourceType = ModelRegistry.GetResourceType(modelType);

            JObject attributes = null;
            if (model != null)
            {
                // attribute bag
                var modelAttributeBag = ModelRegistry.GetAttributeBag(model);
                if (modelAttributeBag != null)
                {
                    attributes = new JObject(modelAttributeBag);
                }
                
                // attributes
                var modelAttributes = ModelRegistry.GetAttributeValues(model);
                if (modelAttributes != null)
                {
                    if (attributes == null)
                    {
                        attributes = modelAttributes;
                    }
                    else
                    {
                        attributes.Merge(modelAttributes, new JsonMergeSettings
                        {
                            MergeNullValueHandling = MergeNullValueHandling.Ignore,
                            MergeArrayHandling = MergeArrayHandling.Replace
                        });
                    }
                }

                // relationships
                // TODO
            }

            var token = Guid.NewGuid();

            StagingArea[token] = new Resource
            {
                Type = resourceType,
                Attributes = attributes
            };

            return token;
        }

        public void UnstageResource(Guid stagingId)
        {
            StagingArea.Remove(stagingId);
        }

        public Resource GetResource(Guid id)
        {
            Resource ret;
            if (!StagingArea.TryGetValue(id, out ret))
            {
                
            }

            return StagingArea[id];
        }

        public void PromoteStagedResource(Guid stagingId, Guid id)
        {
            var stagedResource = GetResource(stagingId);
            stagedResource.Id = id;
            SessionState[id] = stagedResource;
            UnstageResource(stagingId);
        }
    }
}
