using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Linq.Queryables
{
    internal class WhereQueryable<TModel> : RemoteQueryable<TModel>
    {
        private static readonly IDictionary<ExpressionType, string> OpMap = new Dictionary<ExpressionType, string>
        {
            {ExpressionType.Equal, "eq"},
            {ExpressionType.NotEqual, "ne"},
            {ExpressionType.GreaterThan, "gt"},
            {ExpressionType.LessThan, "lt"},
            {ExpressionType.GreaterThanOrEqual, "gte"},
            {ExpressionType.LessThanOrEqual, "lte"}
        };

        private RemoteQueryable<TModel> Target { get; }
        private Expression<Func<TModel, bool>> Predicate { get; }

        public WhereQueryable(
            IQuerySession session,
            RemoteQueryable<TModel> target,
            Expression<Func<TModel, bool>> predicate) :
            base(session, target.Provider)
        {
            Target = target;
            Predicate = predicate;
        }

        public override IQueryContext BuildQuery()
        {
            var bExpression = Predicate?.Body as BinaryExpression;
            if (bExpression == null) throw new NotSupportedException();

            var query = Target.BuildQuery();

            var resourceName = GetJsonName(typeof(TModel));

            query.AppendFilter(resourceName, BuildFilterClause(bExpression));

            return query;
        }

        private string BuildFilterClause(Expression expression)
        {
            var bExpression = expression as BinaryExpression;
            if (bExpression != null)
            {
                if (bExpression.NodeType == ExpressionType.AndAlso)
                {
                    return $"{BuildFilterClause(bExpression.Left)},{BuildFilterClause(bExpression.Right)}";
                }

                if (bExpression.NodeType == ExpressionType.OrElse)
                {
                    return $"{BuildFilterClause(bExpression.Left)},|{BuildFilterClause(bExpression.Right)}";
                }

                string op;
                if (!OpMap.TryGetValue(bExpression.NodeType, out op))
                    throw new NotSupportedException();

                return $"{BuildFilterClause(bExpression.Left)}[{op}]{BuildFilterClause(bExpression.Right)}";
            }

            ConstantExpression cExpression;
            var mExpression = expression as MemberExpression;
            if (mExpression == null)
            {
                cExpression = expression as ConstantExpression;
                if (cExpression == null)
                    throw new NotSupportedException();

                return GetValueLiteral(cExpression.Value);
            }

            if (mExpression.Expression is ParameterExpression)
            {
                return GetJsonName(mExpression.Member);
            }

            var propertyInfo = mExpression.Member as PropertyInfo;
            var fieldInfo = mExpression.Member as FieldInfo;

            if (propertyInfo == null && fieldInfo == null)
            {
                return GetValueLiteral(null);
            }

            cExpression = mExpression.Expression as ConstantExpression;
            if (mExpression.Expression != null && cExpression == null)
                throw new NotSupportedException();

            cExpression = cExpression ?? Expression.Constant(null);

            var value = propertyInfo == null
                ? fieldInfo.GetValue(mExpression.Expression == null ? null : cExpression.Value)
                : mExpression.Expression == null ? propertyInfo.GetValue(null) : propertyInfo.GetValue(cExpression.Value, null);

            return GetValueLiteral(value);
        }

        private static string GetValueLiteral(object value)
        {
            if (value == null) return "NULL";
            if (value is DateTime) return $"'{value:O}'";
            if (value is string || value is char || value is Guid) return $"'{value}'";
            return $"{value}";
        }
    }
}
