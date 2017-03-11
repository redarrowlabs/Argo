using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RedArrow.Argo.Client.Logging;
using RedArrow.Argo.Client.Session.Registry;

namespace RedArrow.Argo.Client.Cache
{
    public class BasicCacheProvider : ICacheProvider
    {
        private static readonly ILog Log = LogProvider.For<BasicCacheProvider>();

        private IDictionary<Guid, object> PoorMansCache { get; }

        private IModelRegistry ModelRegistry { get; }

        public IEnumerable<object> Items => new List<object>(PoorMansCache.Values);

        public BasicCacheProvider(IModelRegistry modelRegistry)
        {
            PoorMansCache = new ConcurrentDictionary<Guid, object>();
            ModelRegistry = modelRegistry;
        }

        public void Update(Guid id, object model)
        {
            object prevModel;
            if (PoorMansCache.TryGetValue(id, out prevModel))
            {
                if (model == prevModel) return;

                Log.Debug(() => $"caching model {{{id}}}");
                var patch = ModelRegistry.GetPatch(prevModel);
                if (patch != null)
                {
                    ModelRegistry.SetPatch(model, patch);
                }
            }
            PoorMansCache[id] = model;
        }

        public TModel Retrieve<TModel>(Guid id)
        {
	        if (!PoorMansCache.ContainsKey(id)) return default(TModel);
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
