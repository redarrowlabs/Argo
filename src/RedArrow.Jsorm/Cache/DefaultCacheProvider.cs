using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using RedArrow.Jsorm.Extensions;
using RedArrow.Jsorm.Infrastructure;
using RedArrow.Jsorm.JsonModels;

namespace RedArrow.Jsorm.Cache
{
    internal class DefaultCacheProvider : ICacheProvider
    {
        private ISet<Type> Container { get; }

        private IDictionary<Guid, Resource> PoorMansCache { get; }

        internal DefaultCacheProvider()
        {
            Container = new HashSet<Type>();
            PoorMansCache = new ConcurrentDictionary<Guid, Resource>();
        }

        public void Register(Type type)
        {
            //TODO: verify that type has a resource mapping?
            Container.Add(type);
        }

        public T New<T>()
        {
            var type = typeof(T);
            if (!Container.Contains(type))
            {
                throw new ModelNotRegisteredException(type);
            }

            var ctor = type.GetDefaultConstructor();
            var model = ctor.Invoke(null);
            return (T)model;
        }

        public Resource Get(Guid id)
        {
			Resource ret;
	        PoorMansCache.TryGetValue(id, out ret);
            return ret;
        }
    }
}