using System;
using System.Collections;
using RedArrow.Argo.Client.Session.Registry;

namespace RedArrow.Argo.Client.Collections
{
    internal abstract class AbstractRemoteCollection : IRemoteCollection
    {
        protected Session.Session Session { get; }
        protected IModelRegistry ModelRegistry => Session.ModelRegistry;

        public object Owner { get; }
        public string Name { get; }
        public bool IsModified { get; protected set; }

        protected bool Initializing { get; set; }

        protected bool Initialized { get; set; }

        protected AbstractRemoteCollection(Session.Session session, object owner, string name)
        {
            Session = session;
            Owner = owner;
            Name = name;
        }

        // TODO: read and write are the same, for now - this will likely change
        protected virtual void Initialize()
        {
            if (Initialized) return;
            if (Initializing) throw new Exception("An attempt was made to initialize a collection already undergoing initialization.");

            Initializing = true;

            Session.InitializeCollection(this);

            Initializing = false;
            Initialized = true;
            IsModified = false;
        }

        public abstract void SetItems(IEnumerable items);

        public abstract IEnumerator GetEnumerator();
        public abstract void CopyTo(Array array, int index);
        public abstract int Count { get; }
        public virtual bool IsSynchronized => false;
        public virtual object SyncRoot => this;
        public virtual bool IsReadOnly => false;
    }
}