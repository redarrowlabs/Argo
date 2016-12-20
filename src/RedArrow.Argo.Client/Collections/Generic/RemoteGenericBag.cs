using System;
using System.Collections.Generic;
using System.Diagnostics;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Infrastructure;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Collections.Generic
{
    [DebuggerTypeProxy(typeof(DebuggerCollectionProxy<>))]
    internal class RemoteGenericBag<T> : AbstractRemoteCollection<T>
        where T : class
    {
        protected IList<T> InternalBag { get; set; }

        public bool Empty => Count == 0;

        public override int Count => InternalBag?.Count ?? 0; // TODO

        internal RemoteGenericBag()
        {
        }

        internal RemoteGenericBag(ICollectionSession session) :
            this(session, new List<T>())
        {
        }

        internal RemoteGenericBag(ICollectionSession session, IEnumerable<T> items) :
            base(session)
        {
            InternalBag = items as IList<T> ?? new List<T>(items);
        }

        public override void Initialize(IEnumerable<T> items)
        {
            Initializing = true; // TODO

            items.Each(x => InternalBag.Add(x));

            Initialized = true; // TODO
            Initializing = false; // TODO
        }

        public override IEnumerator<T> GetEnumerator()
        {
            Read();
            return InternalBag.GetEnumerator();
        }

        public override void Add(T item)
        {
            // TODO: Write()
            InternalBag.Add(item);

            // TODO: queued operation
        }

        public override void Clear()
        {
            // TODO: queued operation

            // TODO: Initialize(true)
            if (InternalBag.Count != 0)
            {
                InternalBag.Clear();
                Dirty = true;
            }
        }

        public override bool Contains(T item)
        {
            // TODO: this will not work with paging
            // TODO: cache hit
            return InternalBag.Contains(item);
        }

        public override bool Remove(T item)
        {
            // TODO: Initialize(true)
            var result = InternalBag.Remove(item);
            Dirty |= result;
            return result;
        }

        public override void CopyTo(T[] array, int index)
        {
            Read();
            for (var i = index; i < Count; i++)
            {
                array.SetValue(InternalBag[i], i);
            }
        }

        public override void CopyTo(Array array, int index)
        {
            Read();
            for (var i = index; i < Count; i++)
            {
                array.SetValue(InternalBag[i], i);
            }
        }
    }
}