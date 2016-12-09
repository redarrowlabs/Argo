using System;
using RedArrow.Jsorm.Client.Session;

namespace RedArrow.Jsorm.Client.Collections
{
    public abstract class AbstractRemoteCollection
    {
        protected ISession Session { get; }

        public object Owner { get; internal set; }

        public bool Dirty { get; protected set; }
        public bool Initialized { get; protected set; }
        public bool Initializing { get; protected set; }
        public virtual bool IsDirectlyAccessible { get; protected set; }

        protected bool IsConnectedToSession => Session != null;

        public abstract bool Empty { get; }

        protected AbstractRemoteCollection() { }

        protected AbstractRemoteCollection(ISession session)
        {
            Session = session;
        }

        protected virtual void SetInitialized()
        {
            Initializing = false;
            Initialized = true;
        }

        public virtual void Read()
        {
            Initialize(false);
        }

        protected virtual void Initialize(bool writing)
        {
            if (!Initialized)
            {
                if (Initializing)
                {
                    throw new Exception("TODO");
                }
                Session.ini
            }
        }
    }
}
