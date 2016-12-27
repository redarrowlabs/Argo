using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using RedArrow.Argo.Client.Collections.Operations;
using RedArrow.Argo.Client.Infrastructure;
using RedArrow.Argo.Client.Session.Patch;

namespace RedArrow.Argo.Client.Collections.Generic
{
    [DebuggerTypeProxy(typeof(DebuggerCollectionProxy<>))]
    internal class RemoteGenericBag<T> : AbstractRemoteCollection, ICollection<T>
        where T : class
    {
        protected IList<T> InternalBag { get; set; }

        public bool Empty => Count == 0;

        public override int Count
        {
            get
            {
                Initialize();
                return InternalBag.Count;
            }
        }

        protected ICollection<IQueuedOperation> QueuedOperations { get; }
        
        internal RemoteGenericBag(Session.Session session) :
            this(session, new List<T>())
        {
        }

        internal RemoteGenericBag(Session.Session session, IEnumerable<T> items) :
            base(session)
        {
            QueuedOperations = new List<IQueuedOperation>();
            InternalBag = items as IList<T> ?? new List<T>(items);
        }
        
        public override void SetItems(IEnumerable items)
        {
            foreach (var item in items)
            {
                InternalBag.Add((T)item);
            }
        }

        public override void Patch(PatchContext patchContext)
        {
            foreach (var op in QueuedOperations)
            {
                op.Patch(patchContext);
            }
        }

        public override void Clean()
        {
            QueuedOperations.Clear();
            Dirty = false;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            Initialize();
            return InternalBag.GetEnumerator();
        }

        public override IEnumerator GetEnumerator()
        {
            IEnumerable<T> self = this;
            return self.GetEnumerator();
        }

        public void Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Initialize();
            InternalBag.Add(item);
            QueuedOperations.Add(new QueuedAddOperation(Session.ModelRegistry, Name, item));
            Dirty = true;
        }

        public void Clear()
        {
            if (InternalBag.Count != 0)
            {
                Initialize();
                InternalBag.Clear();
                QueuedOperations.Add(new QueuedClearOperation(Name));
                Dirty = true;
            }
        }

        public bool Contains(T item)
        {
            // TODO: this will not work with paging
            // TODO: cache hit
            return InternalBag.Contains(item);
        }

        public bool Remove(T item)
        {
            Initialize();
            var result = InternalBag.Remove(item);
            if (result)
            {
                QueuedOperations.Add(new QueuedRemoveOperation(Session.ModelRegistry, Name, item));
                Dirty = true;
            }
            return result;
        }

        public void CopyTo(T[] array, int index)
        {
            Initialize();
            for (var i = index; i < Count; i++)
            {
                array.SetValue(InternalBag[i], i);
            }
        }

        public override void CopyTo(Array array, int index)
        {
            Initialize();
            for (var i = index; i < Count; i++)
            {
                array.SetValue(InternalBag[i], i);
            }
        }
    }
}