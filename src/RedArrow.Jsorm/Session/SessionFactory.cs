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
        //private ICacheProvider CacheProvider { get; }

        private Func<HttpClient> HttpClientFactory { get; }

        internal SessionFactory(
            Func<HttpClient> httpClientFactory,
            IDictionary<Type, string> typeLookup,
            IDictionary<Type, PropertyInfo> idLookup,
            ILookup<Type, PropertyConfiguration> attributeLookup)
        {
            HttpClientFactory = httpClientFactory;
            TypeLookup = typeLookup;
            IdLookup = idLookup;
            AttributeLookup = attributeLookup;
            //CacheProvider = cacheProvider;
        }

        //public void Register(Type modelType)
        //{
        //    CacheProvider.Register(modelType);
        //}

        public ISession CreateSession()
        {
            //TODO
            return new Session(
                HttpClientFactory,
                TypeLookup,
                IdLookup,
                AttributeLookup);
        }
    }
}