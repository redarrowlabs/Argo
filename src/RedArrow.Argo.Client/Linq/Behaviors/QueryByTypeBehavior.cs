using System;
using System.Collections.Generic;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Linq.Behaviors
{
	internal class QueryByTypeBehavior : IQueryBehavior
	{
		public IEnumerable<TModel> ExecuteQuery<TModel>(IQuerySession session, IQueryContext query)
		{
			return session.Query<TModel>(query)
				.GetAwaiter()
				.GetResult();
		}
	}
}
