using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedArrow.Jsorm.Client.Cache;
using RedArrow.Jsorm.Client.Extensions;
using RedArrow.Jsorm.Client.JsonModels;
using RedArrow.Jsorm.Client.Session.Patch;
using RedArrow.Jsorm.Client.Session.Registry;
using RedArrow.Jsorm.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedArrow.Jsorm.Client.Session
{
    public class Session : IModelSession, ISession
    {
        private HttpClient HttpClient { get; }

        private ICacheProvider Cache { get; }

        private IModelRegistry ModelRegistry { get; }

        //TODO: combine these into ResourceRegistry
        private IDictionary<Guid, Resource> ResourceState { get; }

        private IDictionary<Guid, PatchContext> PatchContexts { get; }

        internal bool Disposed { get; set; }

        internal Session(
            Func<HttpClient> httpClientFactory,
            ICacheProvider cache,
            IModelRegistry modelRegistry)
        {
            HttpClient = httpClientFactory();
            Cache = cache;
            ModelRegistry = modelRegistry;

            ResourceState = new Dictionary<Guid, Resource>();
            PatchContexts = new Dictionary<Guid, PatchContext>();
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

            var requestContent = ResourceRootCreate
                .FromAttributes(resourceType, null)
                .ToHttpContent();
            var response = await HttpClient.PostAsync(resourceType, requestContent);

            response.EnsureSuccessStatusCode();
            var id = response.GetResourceId();

            ResourceState[id] = new Resource
            {
                Id = id,
                Type = resourceType
            };
			var model = CreateModel<TModel>(id);
			Cache.Update(id, model);

			return model;
        }

        public async Task<TModel> Create<TModel>(TModel model)
            where TModel : class
        {
            return (TModel)await Create(typeof(TModel), model);
        }

        public async Task<object> Create(Type modelType, object model)
        {
            ThrowIfDisposed();

            var id = ModelRegistry.GetModelId(model);
            if (id != Guid.Empty)
            {
                throw new Exception($"Model {modelType} [{id}] has already been created");
            }

            //TODO: check for transient relationships

            var resourceType = ModelRegistry.GetResourceType(modelType);
            var attributes = JObject.FromObject(ModelRegistry
                .GetModelAttributes(modelType)
                .ToDictionary(
                    x => x.AttributeName,
                    x => x.Property.GetValue(model)));

            var requestContent = ResourceRootCreate
                .FromAttributes(resourceType, attributes)
                .ToHttpContent();
            var response = await HttpClient.PostAsync(resourceType, requestContent);

            response.EnsureSuccessStatusCode();
            id = response.GetResourceId();

			ResourceState[id] = new Resource
			{
				Id = id,
				Type = resourceType,
				Attributes = attributes
			};
			model = CreateModel(modelType, id);
            Cache.Update(id, model);

            return model;
        }

        public async Task<TModel> Get<TModel>(Guid id)
            where TModel : class
        {
            ThrowIfDisposed();

            var model = Cache.Retrieve(id);
            if (model != null)
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

			ResourceState[id] = root.Data;
			model = CreateModel<TModel>(id);
            Cache.Update(id, model);

            return (TModel)model;
        }

        public async Task Update<TModel>(TModel model) where TModel : class
        {
            ThrowIfDisposed();

            var id = ModelRegistry.GetModelId(model);
            var resourceType = ModelRegistry.GetResourceType<TModel>();

            PatchContext context;
            if (!PatchContexts.TryGetValue(id, out context))
            {
                return;
            }

            await Task.WhenAll(context.GetTransientReferences().Select(async kvp =>
            {
                var rltnName = kvp.Key;
                var rltnId = kvp.Value;

                var transientModel = Cache.Retrieve(rltnId);

                var transientModelType = transientModel.GetType();
                transientModel = await Create(transientModelType, transientModel);
                Cache.Remove(rltnId);
                var transientModelId = ModelRegistry.GetModelId(transientModel);
                var transientResourceType = ModelRegistry.GetResourceType(transientModelType);

                context.SetReference(
                    rltnName,
                    transientModelId,
                    transientResourceType,
                    true);
            }).ToArray());

            var patchResource = context.Resource;
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{resourceType}/{id}")
            {
                Content = ResourceRootSingle.FromResource(patchResource)
                    .ToHttpContent()
            };

            var response = await HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            Resource resource;
            if (ResourceState.TryGetValue(id, out resource))
            {
                // this updateds the locally-cached resource
                // TODO: I think we need a better solution here
                if (patchResource.Attributes != null)
                {
                    resource.Attributes.Merge(patchResource.Attributes, new JsonMergeSettings
                    {
                        MergeNullValueHandling = MergeNullValueHandling.Merge,
                        MergeArrayHandling = MergeArrayHandling.Replace
                    });
                }
                patchResource.Relationships?.Each(kvp => resource.GetRelationships()[kvp.Key] = kvp.Value);
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

            Cache.Remove(id);
            ResourceState.Remove(id);
            PatchContexts.Remove(id);
        }

        public TAttr GetAttribute<TModel, TAttr>(Guid id, string attrName)
            where TModel : class
        {
            ThrowIfDisposed();
			
            // check cached resources
            JToken jValue;
            Resource resource;
            if (ResourceState.TryGetValue(id, out resource)
                && resource.Attributes != null
                && resource.Attributes.TryGetValue(attrName, out jValue))
            {
                return jValue.Value<TAttr>();
            }
            return default(TAttr);
        }

        public void SetAttribute<TModel, TAttr>(Guid id, string attrName, TAttr value)
            where TModel : class
        {
            ThrowIfDisposed();

            GetPatchContext<TModel>(id).SetAttriute(attrName, value);
        }

        public TRltn GetReference<TModel, TRltn>(Guid id, string attrName)
            where TModel : class
            where TRltn : class
        {
            ThrowIfDisposed();

            //TODO: check patch context for delta rltn

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

            var context = GetPatchContext<TModel>(id);
            var persisted = rltnId != Guid.Empty;
            if (!persisted)
            {
                rltnId = context.GetReference(attrName) ?? Guid.NewGuid();
            }

            context.SetReference(
                attrName,
                rltnId,
                rltnType,
                persisted);
            Cache.Update(rltnId, rltn);
        }

        public TEnum GetEnumerable<TModel, TEnum, TRltn>(Guid id, string attrName)
            where TModel : class
            where TEnum : IEnumerable<TRltn>
            where TRltn : class
        {
            return default(TEnum);
        }

        public void SetEnumerable<TModel, TEnum, TRltn>(Guid id, string attrName, TEnum value)
            where TModel : class
            where TEnum : IEnumerable<TRltn>
            where TRltn : class
        {
        }

        private TModel CreateModel<TModel>(Guid id)
            where TModel : class
        {
            return (TModel)CreateModel(typeof(TModel), id);
        }

        private object CreateModel(Type type, Guid id)
        {
            return Activator.CreateInstance(type, id, this);
        }

        // wrap this in a PatchContext
        private PatchContext GetPatchContext<TModel>(Guid id)
        {
            PatchContext context;
            if (!PatchContexts.TryGetValue(id, out context))
            {
                var resourceType = ModelRegistry.GetResourceType<TModel>();
                var resource = new Resource { Id = id, Type = resourceType };
                context = new PatchContext(resource);
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