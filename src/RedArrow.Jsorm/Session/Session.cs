using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RedArrow.Jsorm.Cache;
using RedArrow.Jsorm.JsonModels;

namespace RedArrow.Jsorm.Session
{
    public class Session : IModelSession, ISession
    {
		private HttpClient HttpClient { get; }
		private SessionState State { get; }

	    public Session(Action<HttpClient> httpClientFactory)
	    {
		    HttpClient = new HttpClient();
		    httpClientFactory(HttpClient);

			State = new SessionState();
	    }

	    public void Dispose()
        {
			HttpClient.Dispose();

		}

	    public async Task<TModel> GetModel<TModel>(Guid id)
	    {
			//TODO check cache?
			var resource = State.Get<TModel>(id);
		    if (resource == null)
		    {
			    await GetRemoteResource<TModel>(id);
		    }

		    return CreateModel<TModel>(id);
	    }

	    private async Task GetRemoteResource<TModel>(Guid id)
	    {
		    var modelType = typeof (TModel);
		    var resourceName = modelType.Name;
		    var response = await HttpClient.GetAsync($"{resourceName}/{id}");
		    var contentString = await response.Content.ReadAsStringAsync();
		    var resourceRoot = JsonConvert.DeserializeObject<ResourceRoot>(contentString);

		    var resource = resourceRoot.Data.ToObject<Resource>();
		    State.Put(id, resource);
	    }

	    private TModel CreateModel<TModel>(Guid id)
		{
			//TODO Cache
			var modelType = typeof(TModel);
			var wovenCtor = modelType
				.GetTypeInfo()
				.DeclaredConstructors
				.SingleOrDefault(t => t.GetParameters().Length == 2);

			return (TModel)wovenCtor.Invoke(new object[] { id, this });
		}

	    public TAttr GetAttribute<TModel, TAttr>(Guid id, string attrName)
	    {
		    throw new NotImplementedException();
	    }

	    public void SetAttribute<TModel, TAttr>(Guid id, string attrName, TAttr value)
	    {
		    throw new NotImplementedException();
	    }
    }
}