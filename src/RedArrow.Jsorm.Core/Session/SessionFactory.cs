using RedArrow.Jsorm.Core.Registry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Core.Session
{
    public class SessionFactory : ISessionFactory
    {
        private IDictionary<Type, Func<object, object>> IdAccessors { get; }

        private ICacheProvider CacheProvider { get; }

        internal SessionFactory(ICacheProvider cacheProvider)
        {
            CacheProvider = cacheProvider;
            IdAccessors = new ConcurrentDictionary<Type, Func<object, object>>();
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
    }
}