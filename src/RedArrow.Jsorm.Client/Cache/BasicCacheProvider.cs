using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Client.Cache
{
    public class BasicCacheProvider : ICacheProvider
    {
        private IDictionary<Guid, object> PoorMansCache { get; }

        public BasicCacheProvider()
        {
            PoorMansCache = new ConcurrentDictionary<Guid, object>();
        }

        public void Update(Guid id, object model)
        {
            PoorMansCache[id] = model;
        }

        public object Retrieve(Guid id)
        {
            if (PoorMansCache.ContainsKey(id))
            {
                return PoorMansCache[id];
            }
            return null;
        }

        public void Remove(Guid id)
        {
            PoorMansCache.Remove(id);
        }
    }
}
