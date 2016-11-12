using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RedArrow.Jsorm.Map.Id.Generator;
using RedArrow.Jsorm.Registry;

namespace RedArrow.Jsorm.Session
{
    public class SessionFactory : ISessionFactory
    {
        private IDictionary<Type, IIdentifierGenerator> IdGenerators { get; }
        public IDictionary<Type, Func<object, Guid>> IdAccessors { get; }

        private ICacheProvider CacheProvider { get; }

        internal SessionFactory(ICacheProvider cacheProvider)
        {
            CacheProvider = cacheProvider;
            IdAccessors = new ConcurrentDictionary<Type, Func<object, Guid>>();
            IdGenerators = new ConcurrentDictionary<Type, IIdentifierGenerator>();
        }

        public void Register(Type modelType)
        {
            CacheProvider.Register(modelType);
        }

        public ISession CreateSession()
        {
            //TODO
            return new Session();
        }

        public void Register<TModel>(Func<object, Guid> getId)
        {
            IdAccessors[typeof(TModel)] = getId;
        }

        public void Register<TModel>(IIdentifierGenerator generator)
        {
            IdGenerators[typeof(TModel)] = generator;
        }
    }
}