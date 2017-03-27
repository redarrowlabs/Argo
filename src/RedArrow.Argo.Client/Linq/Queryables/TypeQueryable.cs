using System.Linq;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Linq.Queryables
{
    internal class TypeQueryable<TModel> : RemoteQueryable<TModel>
    {
		private string BasePath { get; }

        public TypeQueryable(
			string basePath,
			IQuerySession session, 
			IQueryProvider provider) :
            base(session, provider)
        {
	        BasePath = basePath;
        }

        public override IQueryContext BuildQuery()
        {
            return new QueryContext(BasePath);
        }
    }
}
