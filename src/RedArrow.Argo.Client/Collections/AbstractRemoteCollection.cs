using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Collections
{
    internal abstract class AbstractRemoteCollection<T> : IRemoteCollection<T>, ICollection
        where T : class
    {
        protected ICollectionSession Session { get; }

        public object Owner { get; internal set; }
        public string Name { get; internal set; }

        public bool Dirty { get; protected set; }

        protected bool Initializing { get; set; }

        protected bool Initialized { get; set; }

        protected AbstractRemoteCollection()
        {
        }

        protected AbstractRemoteCollection(ICollectionSession session)
        {
            Session = session;
        }

        protected void Read()
        {
            if (Initialized) return;
            if (Initializing)
            {
                throw new Exception("TODO");
            }
            Session.InitializeCollection(this);
        }

        public abstract void Initialize(IEnumerable<T> items);

        public abstract IEnumerator<T> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public abstract void Add(T item);
        public abstract void Clear();
        public abstract bool Contains(T item);
        public abstract void CopyTo(T[] array, int arrayIndex);
        public abstract bool Remove(T item);
        public abstract void CopyTo(Array array, int index);
        public abstract int Count { get; }
        public virtual bool IsSynchronized => false;
        public virtual object SyncRoot => this;
        public virtual bool IsReadOnly => false;
    }
}
