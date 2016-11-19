using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedArrow.Jsorm.Config;
using RedArrow.Jsorm.Extensions;
using RedArrow.Jsorm.JsonModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedArrow.Jsorm.Session
{
    public class Session : IModelSession, ISession
    {
        private HttpClient HttpClient { get; }

        private IDictionary<Type, string> TypeLookup { get; }
        private IDictionary<Type, PropertyInfo> IdLookup { get; }
        private ILookup<Type, PropertyConfiguration> AttributeLookup { get; }

        //private SessionState State { get; }
        private IDictionary<Guid, object> ModelState { get; }

        private IDictionary<Guid, Resource> ResourceState { get; }

        private IDictionary<Guid, Resource> PatchContexts { get; }

        private bool Disposed { get; set; }

        internal Session(
            Func<HttpClient> httpClientFactory,
            IDictionary<Type, string> typeLookup,
            IDictionary<Type, PropertyInfo> idLookup,
            ILookup<Type, PropertyConfiguration> attributeLookup)
        {
            HttpClient = httpClientFactory();
            TypeLookup = typeLookup;
            IdLookup = idLookup;
            AttributeLookup = attributeLookup;

            //State = new SessionState();

            ModelState = new Dictionary<Guid, object>();
            ResourceState = new Dictionary<Guid, Resource>();
            PatchContexts = new Dictionary<Guid, Resource>();
        }

        public void Dispose()
        {
            HttpClient.Dispose();
            Disposed = true;
        }

        public async Task<TModel> Create<TModel>() where TModel : class
        {
            DisposedCheck();

            var modelType = typeof(TModel);
            var resourceType = TypeLookup[modelType];

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
            DisposedCheck();

            var id = ModelId(model);
            if (!Guid.Empty.Equals(id))
            {
                throw new Exception($"Model {typeof(TModel)} [{id}] has already been created");
            }

            var modelType = typeof(TModel);
            var resourceType = TypeLookup[modelType];
            var attributes = JObject.FromObject(AttributeLookup[modelType]
                .ToDictionary(
                    x => x.AttributeName,
                    x => x.PropertyInfo.GetValue(model)));

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
            DisposedCheck();
            TypeCheck<TModel>();

            // TODO: deal with having a resource, but no model?
            // TODO: deal with having a model, but no resource?
            object model;
            if (ModelState.TryGetValue(id, out model))
            {
                return (TModel)model;
            }

            var resourceType = TypeLookup[typeof(TModel)];
            var response = await HttpClient.GetAsync($"{resourceType}/{id}");
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
            DisposedCheck();
            TypeCheck<TModel>();

            var id = ModelId(model);

            Resource context;
            if (!PatchContexts.TryGetValue(id, out context))
            {
                return;
            }

            var resourceType = TypeLookup[typeof(TModel)];

            var root = ResourceRootSingle.FromResource(context);
            var response = await HttpClient.PatchAsync($"{resourceType}/{id}", root.ToHttpContent());

            response.EnsureSuccessStatusCode();

            PatchContexts.Remove(id);
        }

        public Task Delete<TModel>(TModel model)
            where TModel : class
        {
            DisposedCheck();

            var id = ModelId(model);
            return Delete<TModel>(id);
        }

        public async Task Delete<TModel>(Guid id)
            where TModel : class
        {
            DisposedCheck();
            TypeCheck<TModel>();

            var resourceType = TypeLookup[typeof(TModel)];
            var response = await HttpClient.DeleteAsync($"{resourceType}/{id}");
            response.EnsureSuccessStatusCode();

            ModelState.Remove(id);
            ResourceState.Remove(id);
            PatchContexts.Remove(id);
        }

        public TAttr GetAttribute<TModel, TAttr>(Guid id, string attrName)
            where TModel : class
        {
            DisposedCheck();
            TypeCheck<TModel>();

            Resource resource;
            if (ResourceState.TryGetValue(id, out resource))
            {
                JToken valueToken;
                if (resource.Attributes != null && resource.Attributes.TryGetValue(attrName, out valueToken))
                {
                    return valueToken.Value<TAttr>();
                }
            }
            return default(TAttr);
        }

        public void SetAttribute<TModel, TAttr>(Guid id, string attrName, TAttr value)
            where TModel : class
        {
            DisposedCheck();
            TypeCheck<TModel>();

            Resource context;
            if (!PatchContexts.TryGetValue(id, out context))
            {
                var resourceType = TypeLookup[typeof(TModel)];
                context = new Resource { Id = id, Type = resourceType };
                PatchContexts[id] = context;
            }

            context.GetAttributes()[attrName] = JToken.FromObject(value);
        }

        public TRltn GetRelationship<TModel, TRltn>(Guid id, string attrName)
            where TModel : class
            where TRltn : class
        {
            DisposedCheck();
            TypeCheck<TModel>();
            TypeCheck<TRltn>();

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

        public void SetRelationship<TModel, TRltn>(Guid id, string attrName, TRltn rltn)
            where TModel : class
            where TRltn : class
        {
            DisposedCheck();
            TypeCheck<TModel>();
            TypeCheck<TRltn>();

            var rltnId = ModelId(rltn);
            var rltnType = TypeLookup[typeof(TRltn)];

            GetPatchContext<TModel>(id)
                .GetRelationships()[attrName] = new Relationship
                {
                    Data = JToken.FromObject(new ResourceIdentifier { Id = rltnId, Type = rltnType })
                };
        }

        internal TModel CreateModel<TModel>(Guid id)
            where TModel : class
        {
            return (TModel)Activator.CreateInstance(typeof(TModel), id, this);
        }

        internal Resource GetPatchContext<TModel>(Guid id)
        {
            Resource context;
            if (!PatchContexts.TryGetValue(id, out context))
            {
                var resourceType = TypeLookup[typeof(TModel)];
                context = new Resource { Id = id, Type = resourceType };
                PatchContexts[id] = context;
            }
            return context;
        }

        internal Guid ModelId<TModel>(TModel model)
        {
            PropertyInfo idPropInfo;
            if (!IdLookup.TryGetValue(typeof(TModel), out idPropInfo))
            {
                throw new Exception($"{typeof(TModel).FullName} is not a manged model");
            }

            return (Guid)idPropInfo.GetValue(model);
        }

        internal void DisposedCheck()
        {
            if (Disposed)
            {
                throw new Exception("Session disposed");
            }
        }

        internal void TypeCheck<T>()
        {
            var type = typeof(T);
            if (!TypeLookup.ContainsKey(type))
            {
                throw new Exception($"{type} is not manged by jsorm");
            }
        }
    }
}