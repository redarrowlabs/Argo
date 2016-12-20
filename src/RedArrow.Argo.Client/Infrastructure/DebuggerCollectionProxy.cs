using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RedArrow.Argo.Client.Infrastructure
{
    public class DebuggerCollectionProxy<T>
    {
        private ICollection<T> InternalCollection { get; }

        public DebuggerCollectionProxy(ICollection<T> internalCollection)
        {
            InternalCollection = internalCollection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => InternalCollection.ToArray();
    }
}