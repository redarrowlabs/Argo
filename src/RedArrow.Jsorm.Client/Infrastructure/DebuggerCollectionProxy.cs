using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace RedArrow.Jsorm.Client.Infrastructure
{
    public class DebuggerCollectionProxy
    {
        private ICollection InternalCollection { get; }

        public DebuggerCollectionProxy(ICollection internalCollection)
        {
            InternalCollection = internalCollection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object[] Items
        {
            get
            {
                var items = new object[InternalCollection.Count];
                InternalCollection.CopyTo(items, 0);
                return items;
            }
        }
    }

    public class DebuggerCollectionProxy<T>
    {
        private ICollection<T> InternalCollection { get; }

        public DebuggerCollectionProxy(ICollection<T> internalCollection)
        {
            InternalCollection = internalCollection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                var items = new T[InternalCollection.Count];
                InternalCollection.CopyTo(items, 0);
                return items;
            }
        }
    }
}
