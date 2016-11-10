using RedArrow.Jsorm.Core.Extensions;
using RedArrow.Jsorm.Core.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace RedArrow.Jsorm.Core.Registry
{
    internal class DefaultCacheProvider : ICacheProvider
    {
        private ISet<Type> Container { get; }

        private IDictionary<object, object> PoorMansCache { get; }

        internal DefaultCacheProvider()
        {
            Container = new HashSet<Type>();
            PoorMansCache = new ConcurrentDictionary<object, object>();
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

        public T Get<T>(object id)
        {
            object ret;
            if (PoorMansCache.TryGetValue(id, out ret))
            {
                if (ret.GetType().GetTypeInfo().IsSubclassOf(typeof(T)))
                {
                    return (T)ret;
                }

                throw new ModelTypeMismatchException(typeof(T), ret.GetType());
            }
            return default(T);
        }
    }
}