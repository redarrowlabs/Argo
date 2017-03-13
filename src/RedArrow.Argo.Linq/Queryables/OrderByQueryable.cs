using System;
using System.Linq.Expressions;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Linq.Queryables
{
    internal class OrderByQueryable<TModel, TComparable> : RemoteQueryable<TModel>
    {
        private RemoteQueryable<TModel> Target { get; }
        private Expression<Func<TModel, TComparable>> Compare { get; }

        private bool IsDesc { get; }

        public OrderByQueryable(
            RemoteQueryable<TModel> target,
            Expression<Func<TModel, TComparable>> compare,
            IQuerySession session,
            bool isDesc) : base(session)
        {
            Target = target;
            Compare = compare;

            IsDesc = isDesc;
        }

        public override QueryContext BuildQuery()
        {
            var query = Target.BuildQuery();
            
            var memberExpression = Compare.Body as MemberExpression;

            if(!(memberExpression?.Expression is ParameterExpression)) throw new NotSupportedException();

            var sortMember = GetJsonName(memberExpression.Member);
            
            query.AppendSort(sortMember, IsDesc);

            return query;
        }
    }
}
