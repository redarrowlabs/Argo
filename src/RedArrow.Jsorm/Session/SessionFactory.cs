using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using RedArrow.Jsorm.Map.Id.Generator;
using RedArrow.Jsorm.Registry;

namespace RedArrow.Jsorm.Session
{
    public class SessionFactory : ISessionFactory
    {
        private IDictionary<Type, Func<Guid>> IdGenerators { get; }
        public IDictionary<Type, MethodInfo> IdAccessors { get; }
		public IDictionary<Type, MethodInfo> IdMutators { get; }

		private ICacheProvider CacheProvider { get; }

        internal SessionFactory(ICacheProvider cacheProvider)
        {
            CacheProvider = cacheProvider;

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
            return new Session();
        }
    }
}