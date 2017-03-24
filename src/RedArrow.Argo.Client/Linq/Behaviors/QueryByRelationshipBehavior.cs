using System;
using System.Collections.Generic;
using RedArrow.Argo.Client.Query;

namespace RedArrow.Argo.Client.Linq.Behaviors
{
	internal class QueryByRelationshipBehavior : IQueryBehavior
	{
		public IEnumerator<TModel> ExecuteQuery<TModel>(IQueryContext query)
		{
			throw new NotImplementedException();
		}
	}
}
