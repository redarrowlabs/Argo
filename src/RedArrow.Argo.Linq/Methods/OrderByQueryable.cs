using System;
using System.Linq.Expressions;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Linq.Methods
{
    internal class OrderByQueryable<TModel, TComparable> : RemoteQueryable<TModel>
    {
        private RemoteQueryable<TModel> Target { get; }
        private Expression<Func<TModel, TComparable>> Compare { get; }

        private bool IsDesc { get; }
        private bool IsThenBy { get; }

        public OrderByQueryable(
            RemoteQueryable<TModel> target,
            Expression<Func<TModel, TComparable>> compare,
            IQuerySession session,
            bool isDesc,
            bool isThenBy) : base(session)
        {
            Target = target;
            Compare = compare;

            IsDesc = isDesc;
            IsThenBy = isThenBy;
        }

        public override QueryContext BuildQuery()
        {
            var query = Target.BuildQuery();
            
            var memberExpression = Compare.Body as MemberExpression;

            if(!(memberExpression?.Expression is ParameterExpression)) throw new NotSupportedException();

            var sortMember = GetJsonName(memberExpression.Member);

            if (IsDesc)
            {
                sortMember = sortMember.Insert(0, "-");
            }
            if (IsThenBy)
            {
                sortMember = sortMember.Insert(0, ",");
            }

            query.SortBuilder.Append(sortMember);

            return query;
        }
    }
}
