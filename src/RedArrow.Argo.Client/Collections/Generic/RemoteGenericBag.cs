using RedArrow.Argo.Client.Exceptions;
using RedArrow.Argo.Client.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RedArrow.Argo.Client.Collections.Generic
{
    internal class RemoteGenericBag<TItem> : AbstractRemoteCollection, ICollection<TItem>
    {
        protected ICollection<Guid> Ids { get; } = new List<Guid>();

        public bool Empty => Count == 0;

        public override int Count
        {
            get
            {
                Initialize();
                return Ids.Count;
            }
        }

        internal RemoteGenericBag(Session.Session session, object owner, string name, IEnumerable<TItem> items = null) :
            base(session, owner, name)
        {
            if (items == null)
            {
                // pull from owner resource
                var relationships = ModelRegistry.GetResource(owner).Relationships;
                if (relationships != null && relationships.TryGetValue(name, out var rltn))
                {
                    var ids = rltn.Data?.SelectTokens("[*].id");
                    ids.Each(id => Ids.Add(id.ToObject<Guid>()));
                }
            }
            else
            {
                items.Each(Add);
            }
        }

        // should only be used by session when loading remotely, so we can assume these models are cached
        public override void SetItems(IEnumerable items)
        {
            if (items == null) return;

            foreach (var item in items.OfType<TItem>())
            {
                AddInternal(item);
            }
        }

        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
        {
            Initialize();
            using (var itr = Ids.GetEnumerator())
            {
                while (itr.MoveNext())
                {
                    var id = itr.Current;
                    // TODO: hit cache diectly? (cache hit is not async)
                    yield return Session.Get<TItem>(id).GetAwaiter().GetResult();
                }
            }
        }

        public override IEnumerator GetEnumerator()
        {
            IEnumerable<TItem> self = this;
            return self.GetEnumerator();
        }

        public void Add(TItem item)
        {
            if (item == null) return;

            Initialize();

            AddInternal(item);
        }

        private void AddInternal(TItem item)
        {
            if (item == null) return;

            if (ModelRegistry.IsManagedModel(item) && !ModelRegistry.IsManagedBy(Session, item))
            {
                var id = ModelRegistry.GetId(item);
                throw new UnmanagedModelException(item.GetType(), id);
            }

            var itemId = ModelRegistry.GetOrCreateId(item);
            Session.Cache.Update(itemId, item);

            if (Ids.Contains(itemId)) return;

            Ids.Add(itemId);
            IsModified = true;
        }

        public void Clear()
        {
            if (Ids.Count != 0)
            {
                Initialize();
                Ids.Clear();
                IsModified = true;
            }
        }

        public bool Contains(TItem item)
        {
            if (item == null) return false;

            Initialize();

            return Ids.Contains(ModelRegistry.GetId(item));
        }

        public bool Remove(TItem item)
        {
            if (item == null) return true;

            Initialize();

            var itemId = ModelRegistry.GetId(item);

            var removed = Ids.Remove(itemId);
            if (removed)
            {
                IsModified = true;
            }
            return removed;
        }

        public void CopyTo(TItem[] array, int index)
        {
            Initialize();
            var bag = Ids.ToArray();
            for (var i = index; i < Count; i++)
            {
                var id = bag[i];
                var model = Session.Get<TItem>(id).GetAwaiter().GetResult();
                array.SetValue(model, i);
            }
        }

        public override void CopyTo(Array array, int index)
        {
            Initialize();
            var bag = Ids.ToArray();
            for (var i = index; i < Count; i++)
            {
                var id = bag[i];
                var model = Session.Get<TItem>(id).GetAwaiter().GetResult();
                array.SetValue(model, i);
            }
        }
    }
}