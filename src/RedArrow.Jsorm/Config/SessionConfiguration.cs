using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using RedArrow.Jsorm.Cache;
using RedArrow.Jsorm.Session;

namespace RedArrow.Jsorm.Config
{
    public class SessionConfiguration
    {
        //internal ICacheProvider CacheProvider { get; set; }
	    internal Func<HttpClient> HttpClientFactory { get; set; }

		internal IDictionary<Type, PropertyInfo> IdProperties { get; set; } 

	    internal SessionConfiguration()
        {
            //CacheProvider = new DefaultCacheProvider();
        }

        public ISessionFactory BuildSessionFactory()
        {
	        return new SessionFactory(
		        HttpClientFactory,
		        IdProperties);
        }
    }
}