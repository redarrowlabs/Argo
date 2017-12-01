using System;
using System.Linq.Expressions;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Linq.Queryables
{
    internal class TakeQueryable<TModel> : RemoteQueryable<TModel>
    {
        private RemoteQueryable<TModel> Target { get; }
        private Expression Take { get; }

        public TakeQueryable(
            IQuerySession session,
            RemoteQueryable<TModel> target,
            Expression take) :
            base(session, target.Provider)
        {
            Target = target;
            Take = take;
        }

        public override IQueryContext BuildQuery()
        {
            var cExpression = Take as ConstantExpression;
            if (cExpression == null)
                throw new NotSupportedException();

            var query = Target.BuildQuery();

            query.PageLimit = (int) cExpression.Value;

            return query;
        }
    }
}