using System;
using System.Collections;
using RedArrow.Argo.Client.Session.Patch;

namespace RedArrow.Argo.Client.Collections
{
    internal abstract class AbstractRemoteCollection : IRemoteCollection
    {
        protected Session.Session Session { get; }
        
        public object Owner { get; internal set; }
        public string Name { get; internal set; }
     
        public bool Dirty { get; protected set; }

        protected bool Initializing { get; set; }

        protected bool Initialized { get; set; }
        
        protected AbstractRemoteCollection()
        {
        }

        protected AbstractRemoteCollection(Session.Session session)
        {
            Session = session;
        }

        // TODO: read and write are the same, for now - this will likely change
        protected virtual void Initialize()
        {
            if (Initialized) return;
            if (Initializing) throw new Exception("TODO");

            Session.InitializeCollection(this);
        }

        public abstract void SetItems(IEnumerable items);
        public abstract void Patch(PatchContext patchContext);
        public abstract void ClearOperationQueue();

        public abstract IEnumerator GetEnumerator();
        public abstract void CopyTo(Array array, int index);
        public abstract int Count { get; }
        public virtual bool IsSynchronized => false;
        public virtual object SyncRoot => this;
        public virtual bool IsReadOnly => false;
    }
}
