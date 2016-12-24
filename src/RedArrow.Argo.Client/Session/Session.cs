using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Cache;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.JsonModels;
using RedArrow.Argo.Client.Session.Patch;
using RedArrow.Argo.Client.Session.Registry;
using RedArrow.Argo.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Collections;
using RedArrow.Argo.Client.Collections.Generic;
using RedArrow.Argo.Client.Http;
using RedArrow.Argo.Client.Logging;

namespace RedArrow.Argo.Client.Session
{
    public class Session : IModelSession, ISession, ICollectionSession
    {
        private static readonly ILog Log = LogProvider.For<Session>();

        private HttpClient HttpClient { get; }

        private IHttpRequestBuilder HttpRequestBuilder { get; }

        internal ICacheProvider Cache { get; }

        internal IModelRegistry ModelRegistry { get; }

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
            return (TModel) await CreateResource(typeof(TModel), null);
        }

        public async Task<TModel> Create<TModel>(TModel model)
            where TModel : class
        {
            return (TModel) await CreateResource(typeof(TModel), model);
        }

        public Task<object> Create(Type modelType, object model)
        {
            return CreateResource(modelType, model);
        }

        private async Task<object> CreateResource(Type modelType, object model)
        {
            ThrowIfDisposed();

            var createPayload = HttpRequestBuilder.CreateResource(modelType, model);

            Log.Info(
                () =>
                    $"JSORM||creating resource {createPayload.ResourceType} from model {modelType} {JsonConvert.SerializeObject(model)}");

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
                return (TModel) model;
            }

            var requestContext = HttpRequestBuilder.GetResource(id, typeof(TModel));
            var response = await HttpClient.SendAsync(requestContext.Request);
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

            return (TModel) model;
        }

        public async Task Update<TModel>(TModel model)
            where TModel : class
        {
            ThrowIfDisposed();

            var id = ModelRegistry.GetModelId(model);
            var resourceType = ModelRegistry.GetResourceType(typeof(TModel));

            var dirtyCollections = ModelRegistry
                .GetCollectionConfigurations<TModel>()
                .Select(x => x.PropertyInfo.GetValue(model))
                .OfType<IRemoteCollection>()
                .Where(x => x.Dirty)
                .ToArray();

            PatchContext context;
            if (!PatchContexts.TryGetValue(id, out context))
            {
                if (dirtyCollections.Any() && ResourceState.ContainsKey(id))
                {
                    context = new PatchContext(new Resource
                    {
                        Id = id,
                        Type = resourceType
                    });

                    dirtyCollections.Each(x =>
                    {
                        var relationship = ResourceState[id]
                            .GetRelationships()
                            .ContainsKey(x.Name)
                                ? ResourceState[id].Relationships[x.Name]
                                : new Relationship();
                        context.SetRelationship(x.Name, relationship);
                    });
                    PatchContexts[id] = context;
                }
                else
                {
                    return;
                }
            }

            // flush collection ops to patch context
            ModelRegistry
                .GetCollectionConfigurations<TModel>()
                .Select(x => x.PropertyInfo.GetValue(model))
                .OfType<IRemoteCollection>()
                .Each(x => x.Patch(context));

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
            dirtyCollections.Each(x => x.Clean());
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

        public TRltn GetReference<TModel, TRltn>(Guid id, string rltnName)
            where TModel : class
            where TRltn : class
        {
            ThrowIfDisposed();

            //TODO: check patch context for delta rltn

            Resource resource;
            if (ResourceState.TryGetValue(id, out resource))
            {
                Relationship relationship;
                if (resource.Relationships != null && resource.Relationships.TryGetValue(rltnName, out relationship))
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

        public void SetReference<TModel, TRltn>(Guid id, string rltnName, TRltn rltn)
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
                rltnId = context.GetRelated(rltnName) ?? Guid.NewGuid();
            }

            context.SetRelated(
                rltnName,
                rltnId,
                rltnType,
                persisted);
            Cache.Update(rltnId, rltn);
        }

        public void InitializeCollection(IRemoteCollection collection)
        {
            // TODO: determine if {id/type} from owner relationship are cached already
            // TODO: abstract this into a collection loader/initializer

            // TODO: brute force this for now

            // TODO: don't run this task if the resource collection is empty/null!

            Task.Run(async () =>
            {
                var requestContext = HttpRequestBuilder.GetRelated(collection.Owner, collection.Name);
                var response = await HttpClient.SendAsync(requestContext.Request);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return;
                }
                response.EnsureSuccessStatusCode();

                var contentJson = await response.Content.ReadAsStringAsync();
                var root = JsonConvert.DeserializeObject<ResourceRootCollection>(contentJson);

                if (root.Data == null)
                {
                    return;
                }

                // TODO: clean this up

                var items = root.Data.Select(x =>
                {
                    ResourceState[x.Id] = x;
                    var modelType = ModelRegistry.GetModelType(x.Type);
                    if (modelType == null)
                    {
                        // TODO: ModelNotRegisteredException
                        throw new Exception("TODO");
                    }
                    var model = CreateModel(modelType, x.Id);
                    Cache.Update(x.Id, model);
                    return model;
                });
                collection.SetItems(items);
            }).Wait();
        }

        public IEnumerable<TElmnt> GetGenericEnumerable<TModel, TElmnt>(Guid id, string rltnName)
            where TModel : class
            where TElmnt : class
        {
            return GetRemoteCollection<TModel, TElmnt>(id, rltnName);
        }

        public IEnumerable<TElmnt> SetGenericEnumerable<TModel, TElmnt>(Guid id, string attrName,
            IEnumerable<TElmnt> value)
            where TModel : class
            where TElmnt : class
        {
            return SetRemoteCollection<TModel, TElmnt>(id, attrName, value);
        }

        public ICollection<TElmnt> GetGenericCollection<TModel, TElmnt>(Guid id, string rltnName)
            where TModel : class
            where TElmnt : class
        {
            return GetRemoteCollection<TModel, TElmnt>(id, rltnName);
        }

        public ICollection<TElmnt> SetGenericCollection<TModel, TElmnt>(Guid id, string attrName,
            IEnumerable<TElmnt> value)
            where TModel : class
            where TElmnt : class
        {
            return SetRemoteCollection<TModel, TElmnt>(id, attrName, value);
        }

        private RemoteGenericBag<TElmnt> GetRemoteCollection<TModel, TElmnt>(Guid id, string rltnName)
            where TModel : class
            where TElmnt : class
        {
            var owner = Cache.Retrieve(id);

            var rltnConfig = ModelRegistry.GetCollectionConfiguration<TModel>(rltnName);

            // TODO: configure collection based on rltnConfig

            return new RemoteGenericBag<TElmnt>(this)
            {
                Name = rltnName,
                Owner = owner
            };
        }

        private RemoteGenericBag<TElmnt> SetRemoteCollection<TModel, TElmnt>(Guid id, string rltnName,
            IEnumerable<TElmnt> value)
            where TModel : class
            where TElmnt : class
        {
            var owner = Cache.Retrieve(id);

            var rltnConfig = ModelRegistry.GetCollectionConfiguration<TModel>(rltnName);

            // TODO: configure collection based on rltnConfig

            return new RemoteGenericBag<TElmnt>(this, value)
            {
                Name = rltnName,
                Owner = owner
            };
        }

        private TModel CreateModel<TModel>(Guid id)
        {
            return (TModel) CreateModel(typeof(TModel), id);
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
                var patch = new Resource
                {
                    Id = id,
                    Type = resourceType
                };

                context = new PatchContext(patch);
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