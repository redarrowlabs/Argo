using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Linq.Queryables
{
    internal class TypeQueryable<TModel> : RemoteQueryable<TModel>
    {
        public TypeQueryable(IQuerySession session) : base(session)
        {
        }

        public override QueryContext BuildQuery()
        {
            return new QueryContext();
        }
    }
}
