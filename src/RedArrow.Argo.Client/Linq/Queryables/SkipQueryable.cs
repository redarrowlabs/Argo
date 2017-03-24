using System;
using System.Linq.Expressions;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Linq.Queryables
{
    internal class SkipQueryable<TModel> : RemoteQueryable<TModel>
    {
        private RemoteQueryable<TModel> Target { get; }
        private Expression Skip { get; }

        public SkipQueryable(
            IQuerySession session,
            RemoteQueryable<TModel> target,
            Expression skip) :
            base(session, target.Provider, target.Behavior)
        {
            Target = target;
            Skip = skip;
        }

        public override IQueryContext BuildQuery()
        {
            var cExpression = Skip as ConstantExpression;
            if (cExpression == null)
                throw new NotSupportedException();

            var query = Target.BuildQuery();

            query.PageOffset = (int) cExpression.Value;

            return query;
        }
    }
}
