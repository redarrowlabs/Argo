using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using RedArrow.Jsorm.Client.Infrastructure;
using RedArrow.Jsorm.Client.Session;

namespace RedArrow.Jsorm.Client.Collections.Generic
{
    [DebuggerTypeProxy(typeof(DebuggerCollectionProxy<>))]
    public class RemoteGenericBag<T> : AbstractRemoteCollection, ICollection<T>, ICollection
    {
        protected IList<T> InternalBag { get; set; }

	    public bool IsSynchronized => false;

	    public object SyncRoot => this;

	    public bool IsFixedSize => false;

	    public bool IsReadOnly => false;

	    public bool Empty => InternalBag.Count == 0;

        public int Count => InternalBag.Count; // TODO

		internal RemoteGenericBag()
        {
        }

        internal RemoteGenericBag(ICollectionSession session) :
			base(session)
        {
        }

        internal RemoteGenericBag(ICollectionSession session, IEnumerable<T> items) :
            base(session)
		{
			InternalBag = items as IList<T> ?? new List<T>(items);
        }
		

		public IEnumerator<T> GetEnumerator()
		{
		    Read();
		    return InternalBag.GetEnumerator();
	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
		    return GetEnumerator();
		}

		public int Add(object value)
		{
			Add((T)value);
			return InternalBag.Count - 1;
		}

		public void Add(T item)
	    {
			// TODO: Write()
			InternalBag.Add(item);

			// TODO: queued operation
	    }

	    public void Clear()
		{
			// TODO: queued operation

			// TODO: Initialize(true)
		    if (InternalBag.Count != 0)
		    {
			    InternalBag.Clear();
			    Dirty = true;
		    }
	    }

	    public bool Contains(object value)
	    {
		    return Contains((T) value);
	    }

		public bool Contains(T item)
		{
			// TODO: this will not work with paging
			// TODO: cache hit
			return InternalBag.Contains(item);
		}
        
		public void Remove(object value)
		{
			Remove((T) value);
		}

		public bool Remove(T item)
		{
			// TODO: Initialize(true)
			var result = InternalBag.Remove(item);
			Dirty |= result;
			return result;
		}

		public void CopyTo(T[] array, int index)
		{
            throw new NotImplementedException();
		}

		public void CopyTo(Array array, int index)
		{
            throw new NotImplementedException();
		}
	}
}
