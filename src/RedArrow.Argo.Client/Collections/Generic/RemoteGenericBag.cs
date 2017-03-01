﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Exceptions;
using RedArrow.Argo.Client.Model;

namespace RedArrow.Argo.Client.Collections.Generic
{
    internal class RemoteGenericBag<TItem> : AbstractRemoteCollection, ICollection<TItem>
        where TItem : class
    {
        protected ICollection<Guid> Ids { get; set; }

        public bool Empty => Count == 0;

        public override int Count
        {
            get
            {
                Initialize();
                return Ids.Count;
            }
        }
        
        internal RemoteGenericBag(Session.Session session, IEnumerable<TItem> items = null) :
            base(session)
        {
            Ids = new List<Guid>(items?.Select(ModelRegistry.GetOrCreateId) ?? new Guid[0]);
        }
        
        public override void SetItems(IEnumerable items)
        {
            Ids = new List<Guid>(items?.OfType<object>().Select(ModelRegistry.GetOrCreateId) ?? new Guid[0]);
        }
        
        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
        {
            Initialize();
            using (var itr = Ids.GetEnumerator())
            {
                while (itr.MoveNext())
                {
                    var id = itr.Current;
                    // TODO: hit cache instead? (cache hit is not async)
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

            if (ModelRegistry.IsManagedModel(item) && !ModelRegistry.IsManagedBy(Session, item))
            {
                var id = ModelRegistry.GetId(item);
                throw new UnmanagedModelException(item.GetType(), id);
            }
            
            var itemId = ModelRegistry.GetOrCreateId(item);

            if (Ids.Contains(itemId))
            {
                return;
            }

            Session.Cache.Update(itemId, item);
            var resource = ModelRegistry.GetResource(item);
            GetRelationship().Add(resource.ToResourceIdentifier());
        }

        public void Clear()
        {
            if (Ids.Count != 0)
            {
                Initialize();
                Ids.Clear();
                GetRelationship().Clear();
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

            var result = false;

            var itemId = ModelRegistry.GetId(item);
            
            if (Ids.Contains(itemId))
            {
                result = Ids.Remove(itemId);

                var rltn = GetRelationship();
                var rltnIdentifier = rltn.SingleOrDefault(x => x["id"]?.ToObject<Guid>() == itemId);
                rltnIdentifier.Remove();
            }

            return result;
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

        private JArray GetRelationship()
        {
            Relationship rltn;
            var relationships = ModelRegistry.GetPatch(Owner)?.Relationships;
            if (relationships == null || !relationships.TryGetValue(Name, out rltn))
            {
                var modelResource = ModelRegistry.GetResource(Owner);
                relationships = modelResource.Relationships;
                if (relationships == null || !relationships.TryGetValue(Name, out rltn))
                {
                    rltn = new Relationship {Data = new JArray()};
                }

                ModelRegistry.GetOrCreatePatch(Owner).GetRelationships()[Name] = rltn;
            }
            if (rltn.Data == null || rltn.Data.Type == JTokenType.Null)
            {
                rltn.Data = new JArray();
            }
            else if (rltn.Data?.Type != JTokenType.Array)
            {
                throw new ModelMapException(
                    $"Relationship {Name} mapped as [HasMany] but json relationship data was not an array",
                    Owner.GetType(),
                    ModelRegistry.GetId(Owner));
            }
            return (JArray)rltn.Data;
        }
    }
}