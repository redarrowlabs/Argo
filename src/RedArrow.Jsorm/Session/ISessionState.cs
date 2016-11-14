using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedArrow.Jsorm.Session
{
	internal class SessionState
	{
		private IDictionary<Guid, object> State { get;} = new Dictionary<Guid, object>();

		public void Put(Guid id, object model)
		{
			State[id] = model;
		}

		public TModel Get<TModel>(Guid id)
		{
			if (!State.ContainsKey(id))
			{
				return default(TModel);
			}

			return (TModel) State[id];
		}
	}
}
