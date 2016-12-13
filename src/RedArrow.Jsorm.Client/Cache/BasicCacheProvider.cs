using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RedArrow.Jsorm.Client.Logging;

namespace RedArrow.Jsorm.Client.Cache
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
            Log.Debug(() => $"adding model {id} to cache");
            PoorMansCache[id] = model;
        }

        public object Retrieve(Guid id)
        {
            if (PoorMansCache.ContainsKey(id))
            {
                Log.Debug(() => $"retrieved model {id} from cache");
                return PoorMansCache[id];
            }
            return null;
        }

        public void Remove(Guid id)
        {
            Log.Debug(() => $"removing model {id} from cache");
            PoorMansCache.Remove(id);
        }
    }
}
