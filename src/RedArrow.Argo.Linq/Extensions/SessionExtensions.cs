using System.Linq;
using RedArrow.Argo.Client.Session;
using RedArrow.Argo.Linq.Queryables;

namespace RedArrow.Argo.Linq.Extensions
{
	public static class SessionExtensions
	{
		public static IQueryable<TModel> CreateQuery<TModel>(this ISession session)
		{
            return new TypeQueryable<TModel>(session);
		}
	}
}
