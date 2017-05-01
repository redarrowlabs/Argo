using System;
using System.Linq;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Linq.Queryables
{
	internal class RelationshipQueryable<TParent, TModel> : RemoteQueryable<TModel>
	{
		public Guid Id { get; }
		public string RltnName { get; }

		public RelationshipQueryable(
			Guid id,
			string rltnName,
			IQuerySession session,
			IQueryProvider provider) :
            base(session, provider)
		{
			Id = id;
			RltnName = rltnName;
		}

		public override IQueryContext BuildQuery()
		{
			return new RelationshipQueryContext<TParent, TModel>(Id, RltnName);
		}
	}
}
