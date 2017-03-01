using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Exceptions;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Model;

namespace RedArrow.Argo.Client.Collections.Generic
{
    internal class RemoteGenericBag<TItem> : AbstractRemoteCollection, ICollection<TItem>
        where TItem : class
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
			if (items == null) return;

			foreach (var item in items)
			{
				AddInternal(item);
			}
		}
        
		// should only be used by session when loading remotely, so we can assume these models are cached
        public override void AddRange(IEnumerable items)
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

	        if (!AddInternal(item)) return;

	        var rltn = GetOrCreateRelationship();
	        rltn.Add(JObject.FromObject(new ResourceIdentifier
	        {
		        Id = ModelRegistry.GetId(item),
		        Type = ModelRegistry.GetResourceType(item.GetType())
	        }));
        }

	    private bool AddInternal(TItem item)
	    {
		    if (item == null) return false;

			if (ModelRegistry.IsManagedModel(item) && !ModelRegistry.IsManagedBy(Session, item))
			{
				var id = ModelRegistry.GetId(item);
				throw new UnmanagedModelException(item.GetType(), id);
			}

			var itemId = ModelRegistry.GetOrCreateId(item);
			Session.Cache.Update(itemId, item);

			if (Ids.Contains(itemId)) return false;

		    Ids.Add(itemId);
		    return true;
	    }

        public void Clear()
        {
            if (Ids.Count != 0)
            {
                Initialize();
                Ids.Clear();
	            var relationships = ModelRegistry.GetOrCreatePatch(Owner).GetRelationships();
				Relationship rltn;
				if (!relationships.TryGetValue(Name, out rltn))
	            {
		            rltn = new Relationship();
		            relationships[Name] = rltn;
	            }
	            rltn.Data = new JArray();
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
		        var rltn = GetOrCreateRelationship();
		        rltn.SelectToken($"[?(@.id == '{itemId}')]")?.Remove();
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

		private JArray GetOrCreateRelationship()
		{
			Relationship rltn;
			var relationships = ModelRegistry.GetPatch(Owner)?.Relationships;
			if (relationships == null || !relationships.TryGetValue(Name, out rltn))
			{
				var modelResource = ModelRegistry.GetResource(Owner);
				relationships = modelResource.Relationships;
				if (relationships == null || !relationships.TryGetValue(Name, out rltn))
				{
                    //TODO: this shouldn't be necessary, maybe invert the above ifs
					rltn = new Relationship { Data = new JArray() };
				}
				else if (rltn.Data != null && rltn.Data.Type != JTokenType.Array)
				{
					throw new ModelMapException(
						   $"Relationship {Name} mapped as [HasMany] but json relationship data was not an array",
						   Owner.GetType(),
						   ModelRegistry.GetId(Owner));
				}

                rltn = new Relationship
                {
                    Data = rltn.Data != null
                        ? rltn.Data.DeepClone()
                        : new JArray(),
                    Links = rltn.Links,
                    Meta = rltn.Meta
                };
			    ModelRegistry.GetOrCreatePatch(Owner).GetRelationships()[Name] = rltn;
			}
			return (JArray)rltn.Data;
		}
	}
}