using System.Collections.Generic;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Query;

namespace RedArrow.Argo.Client.Linq.Behaviors
{
	internal interface IQueryBehavior
	{
		IEnumerator<TModel> ExecuteQuery<TModel>(IQueryContext query);
	}
}
