using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedArrow.Jsorm.Config;
using RedArrow.Jsorm.Config.Model;
using RedArrow.Jsorm.Extensions;
using RedArrow.Jsorm.JsonModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedArrow.Jsorm.Session
{
    public class Session : IModelSession, ISession
    {
        private HttpClient HttpClient { get; }

        private ModelRegistry ModelRegistry { get; }

        //TODO: refactor these into class(es) that will manage these responsibilities
        private IDictionary<Guid, object> ModelState { get; }

        private IDictionary<Guid, Resource> ResourceState { get; }

        private IDictionary<Guid, Resource> PatchContexts { get; }

        internal bool Disposed { get; set; }

        internal Session(
            Func<HttpClient> httpClientFactory,
            IEnumerable<ModelConfiguration> modelConfigs)
        {
            HttpClient = httpClientFactory();
            ModelRegistry = new ModelRegistry(modelConfigs);

            ModelState = new Dictionary<Guid, object>();
            ResourceState = new Dictionary<Guid, Resource>();
            PatchContexts = new Dictionary<Guid, Resource>();
        }

        public void Dispose()
        {
            HttpClient.Dispose();
            Disposed = true;
        }

        public async Task<TModel> Create<TModel>()
            where TModel : class
        {
            ThrowIfDisposed();

            var resourceType = ModelRegistry.GetResourceType<TModel>();

            var root = ResourceRootCreate.FromAttributes(resourceType, null);
            var response = await HttpClient.PostAsync(resourceType, root.ToHttpContent());

            response.EnsureSuccessStatusCode();
            var id = response.GetResourceId();

            var model = CreateModel<TModel>(id);
            ModelState[id] = model;
            ResourceState[id] = new Resource
            {
                Id = id,
                Type = resourceType
            };

            return model;
        }

        public async Task<TModel> Create<TModel>(TModel model)
            where TModel : class
        {
            ThrowIfDisposed();

            var id = ModelRegistry.GetModelId(model);
            if (!Guid.Empty.Equals(id))
            {
                throw new Exception($"Model {typeof(TModel)} [{id}] has already been created");
            }

            var modelType = typeof(TModel);
            var resourceType = ModelRegistry.GetResourceType(modelType);
            var attributes = JObject.FromObject(ModelRegistry
                .GetModelAttributes(modelType)
                .ToDictionary(
                    x => x.AttributeName,
                    x => x.Property.GetValue(model)));

            var root = ResourceRootCreate.FromAttributes(resourceType, attributes);
            var response = await HttpClient.PostAsync(resourceType, root.ToHttpContent());

            response.EnsureSuccessStatusCode();
            id = response.GetResourceId();

            model = CreateModel<TModel>(id);
            ModelState[id] = model;
            ResourceState[id] = new Resource
            {
                Id = id,
                Type = resourceType,
                Attributes = attributes
            };

            return model;
        }

        public async Task<TModel> Get<TModel>(Guid id)
            where TModel : class
        {
            ThrowIfDisposed();

            object model;
            if (ModelState.TryGetValue(id, out model))
            {
                return (TModel)model;
            }

            var request = ModelRegistry.CreateGetRequest<TModel>(id);
            var response = await HttpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default(TModel); // null
            }
            response.EnsureSuccessStatusCode();

            var contentString = await response.Content.ReadAsStringAsync();
            var root = JsonConvert.DeserializeObject<ResourceRootSingle>(contentString);

            model = CreateModel<TModel>(id);
            ModelState[id] = model;
            ResourceState[id] = root.Data;

            return (TModel)model;
        }

        public async Task Update<TModel>(TModel model) where TModel : class
        {
            ThrowIfDisposed();

            var id = ModelRegistry.GetModelId(model);

            Resource context;
            if (!PatchContexts.TryGetValue(id, out context))
            {
                return;
            }

            var resourceType = ModelRegistry.GetResourceType<TModel>();

            var root = ResourceRootSingle.FromResource(context);
            var response = await HttpClient.PatchAsync($"{resourceType}/{id}", root.ToHttpContent());

            response.EnsureSuccessStatusCode();

            Resource resource;
            if (ResourceState.TryGetValue(id, out resource))
            {
                // this updateds the locally-cached resource
                // TODO: I think we need a better solution here
                if (context.Attributes != null)
                {
                    resource.Attributes.Merge(context.Attributes, new JsonMergeSettings
                    {
                        MergeNullValueHandling = MergeNullValueHandling.Merge,
                        MergeArrayHandling = MergeArrayHandling.Replace
                    });
                }
                context.Relationships?.Each(kvp => resource.GetRelationships()[kvp.Key] = kvp.Value);
            }
            PatchContexts.Remove(id);
        }

        public Task Delete<TModel>(TModel model)
            where TModel : class
        {
            ThrowIfDisposed();

            var id = ModelRegistry.GetModelId(model);
            return Delete<TModel>(id);
        }

        public async Task Delete<TModel>(Guid id)
            where TModel : class
        {
            ThrowIfDisposed();

            var resourceType = ModelRegistry.GetResourceType<TModel>();
            var response = await HttpClient.DeleteAsync($"{resourceType}/{id}");
            response.EnsureSuccessStatusCode();

            ModelState.Remove(id);
            ResourceState.Remove(id);
            PatchContexts.Remove(id);
        }

        public TAttr GetAttribute<TModel, TAttr>(Guid id, string attrName)
            where TModel : class
        {
            ThrowIfDisposed();

            // first check the patch contexts
            JToken valueToken;
            Resource resource;
            if (PatchContexts.TryGetValue(id, out resource) && resource.Attributes.TryGetValue(attrName, out valueToken))
            {
                return valueToken.Value<TAttr>();
            }

            // then check cached resources
            if (ResourceState.TryGetValue(id, out resource) && resource.Attributes.TryGetValue(attrName, out valueToken))
            {
                return valueToken.Value<TAttr>();
            }
            return default(TAttr);
        }

        public void SetAttribute<TModel, TAttr>(Guid id, string attrName, TAttr value)
            where TModel : class
        {
            ThrowIfDisposed();

            GetPatchContext<TModel>(id)
                .GetAttributes()[attrName] = JToken.FromObject(value);
        }

        public TRltn GetReference<TModel, TRltn>(Guid id, string attrName)
            where TModel : class
            where TRltn : class
        {
            ThrowIfDisposed();

            Resource resource;
            if (ResourceState.TryGetValue(id, out resource))
            {
                Relationship relationship;
                if (resource.Relationships != null && resource.Relationships.TryGetValue(attrName, out relationship))
                {
                    var rltnData = relationship.Data;
                    if (rltnData?.Type != JTokenType.Object)
                    {
                        throw new Exception();
                    }

                    var rltnId = rltnData.ToObject<ResourceIdentifier>();
                    return Get<TRltn>(rltnId.Id).GetAwaiter().GetResult();
                }

                return default(TRltn);
            }

            throw new Exception();
        }

        public void SetReference<TModel, TRltn>(Guid id, string attrName, TRltn rltn)
            where TModel : class
            where TRltn : class
        {
            ThrowIfDisposed();

            var rltnId = ModelRegistry.GetModelId(rltn);
            var rltnType = ModelRegistry.GetResourceType<TRltn>();

            GetPatchContext<TModel>(id)
                .GetRelationships()[attrName] = new Relationship
                {
                    Data = JToken.FromObject(new ResourceIdentifier { Id = rltnId, Type = rltnType })
                };
        }

        public TEnum GetEnumerable<TModel, TEnum, TRltn>(Guid id, string attrName)
            where TModel : class
            where TEnum : IEnumerable<TRltn>
            where TRltn : class
        {
            throw new NotImplementedException();
        }

        public void SetEnumerable<TModel, TEnum, TRltn>(Guid id, string attrName, TEnum value)
            where TModel : class
            where TEnum : IEnumerable<TRltn>
            where TRltn : class
        {
            throw new NotImplementedException();
        }

        private TModel CreateModel<TModel>(Guid id)
            where TModel : class
        {
            return (TModel)Activator.CreateInstance(typeof(TModel), id, this);
        }

        private Resource GetPatchContext<TModel>(Guid id)
        {
            Resource context;
            if (!PatchContexts.TryGetValue(id, out context))
            {
                var resourceType = ModelRegistry.GetResourceType<TModel>();
                context = new Resource { Id = id, Type = resourceType };
                PatchContexts[id] = context;
            }
            return context;
        }

        private void ThrowIfDisposed()
        {
            if (Disposed)
            {
                throw new Exception("Session disposed");
            }
        }
    }
}