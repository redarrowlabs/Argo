using RedArrow.Jsorm.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace RedArrow.Jsorm.Session
{
    public class SessionFactory : ISessionFactory
    {
        internal IDictionary<Type, string> TypeLookup { get; }
        internal IDictionary<Type, PropertyInfo> IdLookup { get; }
        internal ILookup<Type, PropertyConfiguration> AttributeLookup { get; }
        internal ILookup<Type, HasOneConfiguration> HasOneLookup { get; }
        internal ILookup<Type, HasManyConfiguration> HasManyLookup { get; }

        private Func<HttpClient> HttpClientFactory { get; }

        internal SessionFactory(
            Func<HttpClient> httpClientFactory,
            IDictionary<Type, string> typeLookup,
            IDictionary<Type, PropertyInfo> idLookup,
            ILookup<Type, PropertyConfiguration> attributeLookup,
            ILookup<Type, HasOneConfiguration> hasOneLookup,
            ILookup<Type, HasManyConfiguration> hasManyLookup)
        {
            HttpClientFactory = httpClientFactory;
            TypeLookup = typeLookup;
            IdLookup = idLookup;
            AttributeLookup = attributeLookup;
            HasOneLookup = hasOneLookup;
            HasManyLookup = hasManyLookup;
        }

	    internal SessionConfiguration BuildConfiguration()
	    {
			return new SessionConfiguration(
			    TypeLookup,
			    IdLookup,
			    AttributeLookup,
			    HasOneLookup,
			    HasManyLookup);
	    }

	    public ISession CreateSession()
        {
	        return new Session(
		        HttpClientFactory,
		        BuildConfiguration());
        }
    }
}