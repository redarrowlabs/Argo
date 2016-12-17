﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Cache;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Http;
using RedArrow.Argo.Client.JsonModels;
using RedArrow.Argo.Client.Logging;
using RedArrow.Argo.Client.Session.Patch;
using RedArrow.Argo.Client.Session.Registry;
using RedArrow.Argo.Session;

namespace RedArrow.Argo.Client.Session
{
    public class Session : IModelSession, ISession
    {
        private static readonly ILog Log = LogProvider.For<Session>();

        private HttpClient HttpClient { get; }

        private IHttpRequestBuilder HttpRequestBuilder { get; }

        private ICacheProvider Cache { get; }

        private IModelRegistry ModelRegistry { get; }

        //TODO: combine these into ResourceRegistry
        private IDictionary<Guid, Resource> ResourceState { get; }

        private IDictionary<Guid, PatchContext> PatchContexts { get; }

        internal bool Disposed { get; set; }

        internal Session(
            Func<HttpClient> httpClientFactory,
            IHttpRequestBuilder httpRequestBuilder,
            ICacheProvider cache,
            IModelRegistry modelRegistry)
        {
            HttpClient = httpClientFactory();
            HttpRequestBuilder = httpRequestBuilder;
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
            return (TModel) await Create(typeof(TModel), null);
        }

        public async Task<TModel> Create<TModel>(TModel model)
            where TModel : class
        {
            return (TModel) await Create(typeof(TModel), model);
        }

        public async Task<object> Create(Type modelType, object model)
        {
            ThrowIfDisposed();
            
            var createPayload = HttpRequestBuilder.CreateResource(modelType, model);

            Log.Info(() => $"JSORM||creating resource {createPayload.ResourceType} from model {modelType} {JsonConvert.SerializeObject(model)}");

            var response = await HttpClient.SendAsync(createPayload.Request);

            response.EnsureSuccessStatusCode();

            var id = response.GetResourceId();
			ResourceState[id] = new Resource
			{
				Id = id,
				Type = createPayload.ResourceType,
				Attributes = createPayload.Attributes
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

            PatchContext context;
            if (!PatchContexts.TryGetValue(id, out context))
            {
                return;
            }

            var requestContext = HttpRequestBuilder.UpdateResource(id, model, context);
            var response = await HttpClient.SendAsync(requestContext.Request);
            response.EnsureSuccessStatusCode();

            Resource resource;
            if (ResourceState.TryGetValue(id, out resource))
            {
                // this updateds the locally-cached resource
                // TODO: we need a better solution here
                if (requestContext.Attributes != null)
                {
                    resource.Attributes.Merge(requestContext.Attributes, new JsonMergeSettings
                    {
                        MergeNullValueHandling = MergeNullValueHandling.Merge,
                        MergeArrayHandling = MergeArrayHandling.Replace
                    });
                }
                requestContext.Relationships?.Each(kvp => resource.GetRelationships()[kvp.Key] = kvp.Value);
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
                        throw new Exception("TODO");
                    }

                    var rltnId = rltnData.ToObject<ResourceIdentifier>();

                    return Task.Run(async () =>
                    {
                        try
                        {
                            return await Get<TRltn>(rltnId.Id);
                        }
                        catch (Exception ex)
                        {
                            Log.FatalException("JSORM||an unexpected error occurred while retrieving a relationship", ex);
                            throw;
                        }
                    }).Result;
                }

                return default(TRltn);
            }

            throw new Exception("TODO");
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
            Log.Debug(() => $"JSORM||instantiating new session-managed instance of {type} with id {id}");
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