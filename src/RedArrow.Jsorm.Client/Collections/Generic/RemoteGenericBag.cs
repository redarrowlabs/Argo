using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RedArrow.Jsorm.Client.Infrastructure;
using RedArrow.Jsorm.Client.Session;

namespace RedArrow.Jsorm.Client.Collections.Generic
{
    [DebuggerTypeProxy(typeof(DebuggerCollectionProxy<>))]
    public class RemoteGenericBag<T> : IList<T>, IList
    {
		protected ISession Session { get; }

        protected IList<T> InternalBag { get; set; }
		
		protected bool Initializing { get; set; }

		protected bool Initialized { get; set; }

		public bool Dirty { get; protected set; }

	    public bool IsSynchronized => false;

	    public object SyncRoot => this;

	    public bool IsFixedSize => false;

	    public bool IsReadOnly => false;

	    public bool Empty => InternalBag.Count == 0;

		public int Count { get; set; } // TODO

		public RemoteGenericBag()
        {
        }

        public RemoteGenericBag(ISession session) :
			this(session, new List<T>())
        {
        }

        public RemoteGenericBag(ISession session, IEnumerable<T> items)
		{
			Session = session;
			InternalBag = items as IList<T> ?? new List<T>(items);
        }
		

		public IEnumerator<T> GetEnumerator()
	    {
			//TODO: Read()
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

		public int IndexOf(object value)
		{
			return IndexOf((T) value);
		}

	    public int IndexOf(T item)
		{
			// TODO: Read()
		    return InternalBag.IndexOf(item);
		}

	    public void Insert(int index, object value)
	    {
		    Insert(index, (T) value);
	    }

	    public void Insert(int index, T item)
	    {
			// TODO: Write()
			InternalBag.Insert(index, item);
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

		public void RemoveAt(int index)
		{
			// TODO: Write()
			InternalBag.RemoveAt(index);
		}

		public void CopyTo(T[] array, int index)
		{
			for (var i = index; i < Count; i++)
			{
				array.SetValue(this[i], i);
			}
		}

		public void CopyTo(Array array, int index)
		{
			for (var i = index; i < Count; i++)
			{
				array.SetValue(this[i], i);
			}
		}

		object IList.this[int index]
		{
			get { return this[index]; }
			set { this[index] = (T) value; }
		}

		public T this[int index]
		{
			get
			{
				// TODO: Read()
				return InternalBag[index];
			}
			set
			{
				// TODO: Write()
				InternalBag[index] = value;
			}
		}
	}
}
