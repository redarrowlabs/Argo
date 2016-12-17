using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RedArrow.Argo.Client.Logging;

namespace RedArrow.Argo.Client.Cache
{
    public class BasicCacheProvider : ICacheProvider
    {
        private static readonly ILog Log = LogProvider.For<BasicCacheProvider>();

        private IDictionary<Guid, object> PoorMansCache { get; }

        public BasicCacheProvider()
        {
            PoorMansCache = new ConcurrentDictionary<Guid, object>();
        }

        public void Update(Guid id, object model)
        {
            Log.Debug(() => $"JSORM||caching model {id}");
            PoorMansCache[id] = model;
        }

        public object Retrieve(Guid id)
        {
            if (PoorMansCache.ContainsKey(id))
            {
                Log.Debug(() => $"JSORM||retrieved cached model {id}");
                return PoorMansCache[id];
            }
            return null;
        }

        public void Remove(Guid id)
        {
            Log.Debug(() => $"JSORM||removing cached model {id}");
            PoorMansCache.Remove(id);
        }
    }
}
