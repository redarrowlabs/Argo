using System;
using System.Collections.Generic;
using RedArrow.Jsorm.JsonModels;

namespace RedArrow.Jsorm.Session
{
	internal class SessionState
	{
		private IDictionary<Guid, ResourceRoot> State { get; } = new Dictionary<Guid, ResourceRoot>();

		public void Put(Guid id, ResourceRoot model)
		{
			State[id] = model;
		}

		public ResourceRoot Get(Guid id)
		{
			ResourceRoot ret;
			return State.TryGetValue(id, out ret)
				? ret
				: null;
		}
	}
}
