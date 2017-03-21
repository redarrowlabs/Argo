using System;
using System.Linq.Expressions;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Linq.Queryables
{
    internal class OrderByQueryable<TModel, TComparable> : RemoteQueryable<TModel>
    {
        private RemoteQueryable<TModel> Target { get; }
        private Expression<Func<TModel, TComparable>> Comparable { get; }
        private bool IsDesc { get; }

        public OrderByQueryable(
            IQuerySession session,
            RemoteQueryable<TModel> target,
            Expression<Func<TModel, TComparable>> comparable,
            bool isDesc) :
            base(session, target.Provider)
        {
            Target = target;
            Comparable = comparable;
            IsDesc = isDesc;
        }

        public override IQueryContext BuildQuery()
        {
            var mExpression = Comparable?.Body as MemberExpression;
            if(!(mExpression?.Expression is ParameterExpression)) throw new NotSupportedException();

            var query = Target.BuildQuery();

            var sortMember = GetJsonName(mExpression.Member);

            if (IsDesc)
            {
                sortMember = sortMember.Insert(0, "-");
            }

            query.AppendSort(sortMember);

            return query;
        }
    }
}
