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
using System.Threading.Tasks;

namespace RedArrow.Jsorm.Session
{
    public class Session : IModelSession, ISession
    {
        private HttpClient HttpClient { get; }
		
		private SessionConfiguration Configuration { get; }

        private IDictionary<Guid, object> ModelState { get; }

        private IDictionary<Guid, Resource> ResourceState { get; }

        private IDictionary<Guid, Resource> PatchContexts { get; }

        internal bool Disposed { get; set; }

        internal Session(
            Func<HttpClient> httpClientFactory,
			SessionConfiguration configuration)
        {
            HttpClient = httpClientFactory();
	        Configuration = configuration;

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

        public async Task<TModel> Create<TModel>()
			where TModel : class
        {
            DisposedCheck();
			
	        var resourceType = Configuration.GetResourceType<TModel>();

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

            var id = Configuration.GetId(model);
            if (!Guid.Empty.Equals(id))
            {
                throw new Exception($"Model {typeof(TModel)} [{id}] has already been created");
            }

            var modelType = typeof(TModel);
            var resourceType = Configuration.GetResourceType(modelType);
            var attributes = JObject.FromObject(Configuration.GetAttributes(modelType)
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
			Configuration.TypeCheck<TModel>();

            // TODO: deal with having a resource, but no model?
            // TODO: deal with having a model, but no resource?
            object model;
            if (ModelState.TryGetValue(id, out model))
            {
                return (TModel)model;
            }

	        var resourceType = Configuration.GetResourceType<TModel>();
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
			Configuration.TypeCheck<TModel>();

            var id = Configuration.GetId(model);

            Resource context;
            if (!PatchContexts.TryGetValue(id, out context))
            {
                return;
            }

	        var resourceType = Configuration.GetResourceType<TModel>();

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
            DisposedCheck();

            var id = Configuration.GetId(model);
            return Delete<TModel>(id);
        }

        public async Task Delete<TModel>(Guid id)
            where TModel : class
        {
            DisposedCheck();
			Configuration.TypeCheck<TModel>();

	        var resourceType = Configuration.GetResourceType<TModel>();
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
			Configuration.TypeCheck<TModel>();

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
            DisposedCheck();
			Configuration.TypeCheck<TModel>();

            GetPatchContext<TModel>(id)
                .GetAttributes()[attrName] = JToken.FromObject(value);
        }

        public TRltn GetRelationship<TModel, TRltn>(Guid id, string attrName)
            where TModel : class
            where TRltn : class
        {
            DisposedCheck();
			Configuration.TypeCheck<TModel>();
			Configuration.TypeCheck<TRltn>();

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
            Configuration.TypeCheck<TModel>();
			Configuration.TypeCheck<TRltn>();

            var rltnId = Configuration.GetId(rltn);
	        var rltnType = Configuration.GetResourceType<TRltn>();

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
	            var resourceType = Configuration.GetResourceType<TModel>();
                context = new Resource { Id = id, Type = resourceType };
                PatchContexts[id] = context;
            }
            return context;
        }

        internal void DisposedCheck()
        {
            if (Disposed)
            {
                throw new Exception("Session disposed");
            }
        }
    }
}