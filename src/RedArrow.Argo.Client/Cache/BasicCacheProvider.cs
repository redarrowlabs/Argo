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

        public IEnumerable<object> Items => new List<object>(PoorMansCache.Values);

        public BasicCacheProvider()
        {
            PoorMansCache = new ConcurrentDictionary<Guid, object>();
        }

        public void Update(Guid id, object model)
        {
            Log.Debug(() => $"caching model {{{id}}}");
            PoorMansCache[id] = model;
        }

        public TModel Retrieve<TModel>(Guid id)
			where TModel : class
        {
	        if (!PoorMansCache.ContainsKey(id)) return null;
	        Log.Debug(() => $"retrieved cached model {{{id}}}");
	        // TODO: verify model.GetType is assignable to TModel
	        return (TModel)PoorMansCache[id];
        }

        public void Remove(Guid id)
        {
            Log.Debug(() => $"removing cached model {{{id}}}");
            PoorMansCache.Remove(id);
        }
    }
}
