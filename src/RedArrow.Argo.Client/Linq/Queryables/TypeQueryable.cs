using System.Linq;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Linq.Queryables
{
    internal class TypeQueryable<TModel> : RemoteQueryable<TModel>
    {
        public TypeQueryable(
            IQuerySession session,
            IQueryProvider provider) :
            base(session, provider)
        {
        }

        public override IQueryContext BuildQuery()
        {
            return new QueryContext<TModel>();
        }
    }
}