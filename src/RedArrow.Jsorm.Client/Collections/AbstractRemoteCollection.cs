using System;
using RedArrow.Jsorm.Client.Session;

namespace RedArrow.Jsorm.Client.Collections
{
    public abstract class AbstractRemoteCollection : IRemoteCollection
    {
        protected ICollectionSession Session { get; }

        public object Owner { get; internal set; }
        public string Name { get; internal set; }

        public bool Dirty { get; protected set; }

        protected bool Initializing { get; set; }

        protected bool Initialized { get; set; }

        protected AbstractRemoteCollection()
        {
        }

        protected AbstractRemoteCollection(ICollectionSession session)
        {
            Session = session;
        }

        protected void Read()
        {
            if (Initialized) return;
            if (Initializing)
            {
                throw new Exception("TODO");
            }
            Session.InitializeCollection(this);
        }
    }
}
