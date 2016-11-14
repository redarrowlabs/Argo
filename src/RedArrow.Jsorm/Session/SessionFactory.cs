using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using RedArrow.Jsorm.Cache;

namespace RedArrow.Jsorm.Session
{
    public class SessionFactory : ISessionFactory
    {
        internal IDictionary<Type, PropertyInfo> IdProperties { get; }

		//private ICacheProvider CacheProvider { get; }

		private Func<HttpClient> HttpClientFactory { get; }

	    internal SessionFactory(
		    Func<HttpClient> httpClientFactory,
		    IDictionary<Type, PropertyInfo> idProperties)
		{
			HttpClientFactory = httpClientFactory;
	        IdProperties = idProperties;
			//CacheProvider = cacheProvider;
        }

        //public void Register(Type modelType)
        //{
        //    CacheProvider.Register(modelType);
        //}

	    public ISession CreateSession()
        {
            //TODO
            return new Session(HttpClientFactory(), IdProperties);
        }
    }
}