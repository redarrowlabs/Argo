using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Linq.Queryables
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
            var query = Target.BuildQuery();

	        var resourceType = typeof(TModel).GetModelResourceType();

            query.AppendFilter(resourceType, TranslateExpression(Predicate?.Body));

            return query;
        }

        private string TranslateExpression(Expression expression)
        {
            if (expression is MethodCallExpression) return TranslateMethodCallExpression(expression);
            if(expression is BinaryExpression) return TranslateBinaryExpression(expression);
            if (expression is MemberExpression) return TranslateMemberExpression(expression);
            throw new NotSupportedException();
        }

        private string TranslateMethodCallExpression(Expression expression)
        {
            var mcExpression = expression as MethodCallExpression;
            if (mcExpression == null || mcExpression.Method.ReturnType != typeof(bool))
                throw new NotSupportedException();

            switch (mcExpression.Method.Name)
            {
                case "Equals":
                {
                    if (mcExpression.Method.GetParameters().Length == 1)
                    {
                        return $"{TranslateExpression(mcExpression.Object)}[eq]{TranslateExpression(mcExpression.Arguments[0])}";
                    }
                    break;
                }
                case "Contains":
                {
                    var paramCount = mcExpression.Method.GetParameters().Length;
                    if (paramCount == 1)
                    {
                        return $"{TranslateExpression(mcExpression.Object)}[cnt]{TranslateExpression(mcExpression.Arguments[0])}";
                    }

                    if (paramCount == 2)
                    {
                        return $"{TranslateExpression(mcExpression.Arguments[0])}[acnt]{TranslateExpression(mcExpression.Arguments[1])}";
                    }
                    break;
                }
                case "StartsWith":
                {
                    return $"{TranslateExpression(mcExpression.Object)}[sw]{TranslateExpression(mcExpression.Arguments[0])}";
                }
                case "EndsWith":
                {
                    return $"{TranslateExpression(mcExpression.Object)}[ew]{TranslateExpression(mcExpression.Arguments[0])}";
                }
            }

            throw new NotSupportedException();
        }

        private string TranslateBinaryExpression(Expression expression)
        {
            var bExpression = expression as BinaryExpression;
            if(bExpression == null) throw new NotSupportedException();
            
            if (bExpression.NodeType == ExpressionType.AndAlso)
            {
                return $"{TranslateExpression(bExpression.Left)},{TranslateExpression(bExpression.Right)}";
            }

            if (bExpression.NodeType == ExpressionType.OrElse)
            {
                return $"{TranslateExpression(bExpression.Left)},|{TranslateExpression(bExpression.Right)}";
            }

            string op;
            if (!OpMap.TryGetValue(bExpression.NodeType, out op))
                throw new NotSupportedException();

            return $"{TranslateExpression(bExpression.Left)}[{op}]{TranslateExpression(bExpression.Right)}";
        }

        private string TranslateMemberExpression(Expression expression)
        {
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
