using RedArrow.Jsorm.JsonModels;
using System;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Session
{
    internal class SessionState
    {
        private IDictionary<Guid, Resource> State { get; } = new Dictionary<Guid, Resource>();

        public void Put(Guid id, Resource model)
        {
            State[id] = model;
        }

        public Resource Get(Guid id)
        {
            Resource ret;
            return State.TryGetValue(id, out ret)
                ? ret
                : null;
        }
    }
}