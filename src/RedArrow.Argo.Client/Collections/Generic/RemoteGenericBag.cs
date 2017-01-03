using System;
using System.Collections;
using System.Collections.Generic;
using RedArrow.Argo.Client.Collections.Operations;
using RedArrow.Argo.Client.Session.Patch;

namespace RedArrow.Argo.Client.Collections.Generic
{
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
            if (item == null) return;

            Initialize();
            InternalBag.Add(item);
            var itemId = Session.ModelRegistry.GetModelId(item);
            var itemResourceType = Session.ModelRegistry.GetResourceType(item.GetType());
            QueuedOperations.Add(new QueuedAddOperation(Name, itemId, itemResourceType));
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
            if (item == null) return false;

            Initialize();
            return InternalBag.Contains(item);
        }

        public bool Remove(T item)
        {
            if (item == null) return true;

            Initialize();
            var result = InternalBag.Remove(item);
            if (result)
            {
                var itemId = Session.ModelRegistry.GetModelId(item);
                QueuedOperations.Add(new QueuedRemoveOperation(Name, itemId));
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