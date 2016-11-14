using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using RedArrow.Jsorm.Cache;
using RedArrow.Jsorm.Map.Id.Generator;

namespace RedArrow.Jsorm.Session
{
    public class SessionFactory : ISessionFactory
    {
        private IDictionary<Type, Func<Guid>> IdGenerators { get; }
        internal IDictionary<Type, MethodInfo> IdAccessors { get; }
		internal IDictionary<Type, MethodInfo> IdMutators { get; }

		private ICacheProvider CacheProvider { get; }

		private Action<HttpClient> HttpClientFactory { get; }

        internal SessionFactory(ICacheProvider cacheProvider, Action<HttpClient> httpClientFactory)
        {
            CacheProvider = cacheProvider;
			HttpClientFactory = httpClientFactory;

			IdGenerators = new ConcurrentDictionary<Type, Func<Guid>>();
			IdAccessors = new ConcurrentDictionary<Type, MethodInfo>();
			IdMutators = new ConcurrentDictionary<Type, MethodInfo>();
        }

        public void Register(Type modelType)
        {
            CacheProvider.Register(modelType);
        }

	    public void RegisterIdAccessor<TModel>(MethodInfo getId)
	    {
		    IdAccessors[typeof (TModel)] = getId;
	    }

	    public void RegisterIdMutator<TModel>(MethodInfo setId)
	    {
		    IdMutators[typeof (TModel)] = setId;
	    }

	    public void RegisterIdGenerator<TModel>(Func<Guid> generator)
	    {
		    IdGenerators[typeof (TModel)] = generator;
	    }

	    public ISession CreateSession()
        {
            //TODO
            return new Session(HttpClientFactory);
        }
    }
}