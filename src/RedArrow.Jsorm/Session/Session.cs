using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RedArrow.Jsorm.JsonModels;

namespace RedArrow.Jsorm.Session
{
    public class Session : IModelSession, ISession
    {
		private HttpClient HttpClient { get; }
		private IDictionary<Type, PropertyInfo> IdProperties { get; } 
		
		private SessionState State { get; }

	    public Session(
			HttpClient httpClient,
			IDictionary<Type, PropertyInfo> idProperties)
	    {
		    HttpClient = httpClient;
		    IdProperties = new ReadOnlyDictionary<Type, PropertyInfo>(idProperties);

			State = new SessionState();
	    }

	    public void Dispose()
        {
			HttpClient.Dispose();
		}

	    public async Task<TModel> Get<TModel>(Guid id)
	    {
			//TODO check cache?
			var resource = State.Get(id);
		    if (resource == null)
		    {
			    await GetRemoteResource<TModel>(id);
		    }

			// TODO: cache these so we're not creating a new one on every get
		    return CreateModel<TModel>(id);
	    }

	    private async Task GetRemoteResource<TModel>(Guid id)
	    {
		    var modelType = typeof (TModel);
		    var resourceName = modelType.Name;
		    var response = await HttpClient.GetAsync($"{resourceName}/{id}");
		    var contentString = await response.Content.ReadAsStringAsync();
		    var resourceRoot = JsonConvert.DeserializeObject<ResourceRoot>(contentString);
		    State.Put(id, resourceRoot);
	    }

	    private TModel CreateModel<TModel>(Guid id)
	    {
		    return (TModel) Activator.CreateInstance(typeof (TModel), id, this);
		}

	    public TAttr GetAttribute<TModel, TAttr>(Guid id, string attrName)
	    {
		    var resourceRoot = State.Get(id);
		    if (resourceRoot == null)
		    {
			    //TODO: something has gone wrong
			    return default(TAttr);
		    }

			// this is probably much cheaper than converting to a Resource object
		    var attrToken = resourceRoot.Data.SelectToken($"$.attributes.{attrName}");
		    if (attrToken == null)
		    {
			    //TODO: something has probably gone wrong
			    return default(TAttr);
		    }

		    return attrToken.ToObject<TAttr>();
	    }

	    public void SetAttribute<TModel, TAttr>(Guid id, string attrName, TAttr value)
	    {
		    throw new NotImplementedException();
	    }
    }
}