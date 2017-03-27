using System.Collections.Generic;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Linq.Behaviors
{
	public interface IQueryBehavior
	{
		IEnumerable<TModel> ExecuteQuery<TModel>(IQuerySession session, IQueryContext query);
	}
}
