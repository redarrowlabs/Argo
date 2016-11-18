using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedArrow.Jsorm.Attributes;
using RedArrow.Jsorm.Config;
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
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }

        public async Task<TModel> Create<TModel>(TModel model)
            where TModel : class
        {
            PropertyInfo idPropInfo;
            if (!IdLookup.TryGetValue(typeof(TModel), out idPropInfo))
            {
                throw new Exception($"{typeof(TModel).FullName} is not a manged model");
            }

            var idVal = idPropInfo.GetValue(model);
            if (!Guid.Empty.Equals(idVal))
            {
                throw new Exception($"Model {typeof(TModel)} [{idVal}] has already been created");
            }

            return await CreateRemoteResource(model);
        }

        private async Task<TModel> CreateRemoteResource<TModel>(TModel model)
            where TModel : class
        {
            var modelType = typeof(TModel);
            var resourceType = TypeLookup[modelType];

            var resourceCreate = new ResourceCreate
            {
                Type = resourceType,
                Attributes = JObject.FromObject(AttributeLookup[modelType]
                    .ToDictionary(
                        x => x.AttributeName,
                        x => x.PropertyInfo.GetValue(model)),
                    JsonSerializer.CreateDefault(new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }))
            };
            var resourceRequest = new ResourceRootCreate
            {
                Data = resourceCreate
            };

            var body = new StringContent(resourceRequest.ToJson(), Encoding.UTF8, "application/vnd.api+json");
            var response = await HttpClient.PostAsync(resourceType, body);

            response.EnsureSuccessStatusCode(); //TODO

            var locationHeader = response.Headers.Location.ToString();
            var idStr = locationHeader.Substring(locationHeader.Length - 36, 36);
            var id = Guid.Parse(idStr);

            model = CreateModel<TModel>(id);
            ModelState[id] = model;
            //TODO
            ResourceState[id] = new Resource
            {
                Id = id,
                Type = resourceCreate.Type,
                Attributes = resourceCreate.Attributes
            };

            return model;
        }

        public async Task<TModel> Get<TModel>(Guid id)
            where TModel : class
        {
			TypeCheck<TModel>();
            // TODO: deal with having a resource, but no model?
            // TOOD: deal with having a model, but no resource?
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
            var resourceRoot = JsonConvert.DeserializeObject<ResourceRootSingle>(contentString);

            model = CreateModel<TModel>(id);
            ModelState[id] = model;
            ResourceState[id] = resourceRoot.Data;

            return (TModel)model;
        }

        public Task Delete<TModel>(TModel model)
            where TModel : class
        {
            PropertyInfo idPropInfo;
            if (!IdLookup.TryGetValue(typeof(TModel), out idPropInfo))
            {
                throw new Exception($"{typeof(TModel).FullName} is not a manged model");
            }

            var idVal = (Guid)idPropInfo.GetValue(model);
            return Delete<TModel>(idVal);
        }

        public async Task Delete<TModel>(Guid id)
            where TModel : class
        {
			TypeCheck<TModel>();
            var resourceType = TypeLookup[typeof(TModel)];
            var response = await HttpClient.DeleteAsync($"{resourceType}/{id}");
            response.EnsureSuccessStatusCode();
            ModelState.Remove(id);
            ResourceState.Remove(id);
        }

        private TModel CreateModel<TModel>(Guid id)
            where TModel : class
        {
            return (TModel)Activator.CreateInstance(typeof(TModel), id, this);
        }

        public TAttr GetAttribute<TModel, TAttr>(Guid id, string attrName)
            where TModel : class
        {
			TypeCheck<TModel>();
			TypeCheck<TAttr>();

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
			TypeCheck<TModel>();
            //TODO
        }

	    public TRltn GetRelationship<TModel, TRltn>(Guid id, string attrName)
			where TModel : class
			where TRltn : class
		{
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

	    public void SetRelationship<TModel, TRltn>(Guid id, string attrName, TRltn rltn) where TModel : class where TRltn : class
	    {
			//TODO
	    }

	    private void TypeCheck<T>()
	    {
		    var type = typeof (T);
		    if(!TypeLookup.ContainsKey(type))
		    {
			    throw new Exception($"{type} is not manged by jsorm");
		    }
	    }
    }
}