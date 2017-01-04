using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RedArrow.Argo.Client.Collections.Operations;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Session.Patch;

namespace RedArrow.Argo.Client.Collections.Generic
{
    internal class RemoteGenericBag<T> : AbstractRemoteCollection, ICollection<T>
        where T : class
    {
        protected IDictionary<Guid, T> InternalIndex { get; set; }

        public bool Empty => Count == 0;

        public override int Count
        {
            get
            {
                Initialize();
                return InternalIndex.Count;
            }
        }

        protected ICollection<IQueuedOperation> QueuedOperations { get; }
        
        internal RemoteGenericBag(Session.Session session, IEnumerable<T> items = null) :
            base(session)
        {
            QueuedOperations = new List<IQueuedOperation>();
            InternalIndex = new Dictionary<Guid, T>();
            IndexItems(items);
        }
        
        public override void SetItems(IEnumerable items)
        {
            IndexItems(items?.OfType<T>());
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
            return InternalIndex.Values.GetEnumerator();
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

            IndexItems(item);
            var itemId = Session.ModelRegistry.GetModelId(item);
            var itemResourceType = Session.ModelRegistry.GetResourceType(item.GetType());
            QueuedOperations.Add(new QueuedAddOperation(Name, itemId, itemResourceType));
            Dirty = true;
        }

        public void Clear()
        {
            if (InternalIndex.Count != 0)
            {
                Initialize();
                InternalIndex.Clear();
                QueuedOperations.Add(new QueuedClearOperation(Name));
                Dirty = true;
            }
        }

        public bool Contains(T item)
        {
            if (item == null) return false;

            Initialize();
            
            return InternalIndex.ContainsKey(GetItemId(item));
        }

        public bool Remove(T item)
        {
            if (item == null) return true;

            Initialize();

            var result = false;

            var itemId = Session.ModelRegistry.GetModelId(item);

            T itemToRemove;
            if (InternalIndex.TryGetValue(itemId, out itemToRemove))
            {
                result = InternalIndex.Remove(itemId);
                if (result)
                {
                    QueuedOperations.Add(new QueuedRemoveOperation(Name, itemId));
                    Dirty = true;
                }
            }
            return result;
        }

        public void CopyTo(T[] array, int index)
        {
            Initialize();
            var bag = InternalIndex.Values.ToList();
            for (var i = index; i < Count; i++)
            {
                array.SetValue(bag[i], i);
            }
        }

        public override void CopyTo(Array array, int index)
        {
            Initialize();
            var bag = InternalIndex.Values.ToList();
            for (var i = index; i < Count; i++)
            {
                array.SetValue(bag[i], i);
            }
        }

        private Guid GetItemId(T item)
        {
            return Session.ModelRegistry.GetModelId(item);
        }

        private void IndexItems(IEnumerable<T> items)
        {
            IndexItems(items?.ToArray());
        }

        private void IndexItems(params T[] items)
        {
            items?.Each(x => InternalIndex[GetItemId(x)] = x);
        }
    }
}