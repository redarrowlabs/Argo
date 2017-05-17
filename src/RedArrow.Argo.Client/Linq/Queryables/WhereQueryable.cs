using Newtonsoft.Json;
using RedArrow.Argo.Client.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RedArrow.Argo.Client.Linq.Queryables
{
    internal abstract class WhereQueryable<TModel> : RemoteQueryable<TModel>
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

        private JsonSerializerSettings JsonSettings { get; }

        protected RemoteQueryable<TModel> Target { get; }
        protected Expression<Func<TModel, bool>> Predicate { get; }

        public WhereQueryable(
            IQuerySession session,
            RemoteQueryable<TModel> target,
            Expression<Func<TModel, bool>> predicate,
            JsonSerializerSettings jsonSettings) :
            base(session, target.Provider)
        {
            Target = target;
            Predicate = predicate;
            JsonSettings = jsonSettings;
        }

        public virtual Func<MemberExpression, bool> IsValidMemberExpression => me => true;

        protected string TranslateExpression(Expression expression)
        {
            if (expression is BinaryExpression) return TranslateBinaryExpression(expression);
            if (expression is MethodCallExpression) return TranslateMethodCallExpression(expression);
            if (expression is MemberExpression) return TranslateMemberExpression(expression);
            if (expression is ConstantExpression) return TranslateMemberExpression(expression);
            throw new NotSupportedException();
        }

        private string TranslateMethodCallExpression(Expression expression)
        {
            var mcExpression = expression as MethodCallExpression;
            if (mcExpression == null)
                throw new NotSupportedException();

            if (mcExpression.Method.ReturnType != typeof(bool))
            {
                var value = GetExpressionValue(null, mcExpression);
                return GetValueLiteral(value);
            }
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

            if (mExpression.Expression.NodeType == ExpressionType.Parameter)
            {
                if (!IsValidMemberExpression(mExpression))
                {
                    throw new NotSupportedException();
                }

                return GetJsonName(mExpression.Member);
            }
            else if (mExpression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                var expressionValue = GetExpressionValue(null, mExpression);
                return GetValueLiteral(expressionValue);
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
                ? fieldInfo.GetValue(mExpression.Expression == null
                    ? null
                    : cExpression.Value)
                : mExpression.Expression == null
                    ? propertyInfo.GetValue(null)
                    : propertyInfo.GetValue(cExpression.Value, null);

            return GetValueLiteral(value);
        }

        private string TranslateBinaryExpression(Expression expression)
        {
            var bExpression = expression as BinaryExpression;
            if (bExpression == null) throw new NotSupportedException();

            if (bExpression.NodeType == ExpressionType.AndAlso)
            {
                return $"({TranslateExpression(bExpression.Left)},{TranslateExpression(bExpression.Right)})";
            }

            if (bExpression.NodeType == ExpressionType.OrElse)
            {
                return $"({TranslateExpression(bExpression.Left)},|{TranslateExpression(bExpression.Right)})";
            }

            string op;
            if (!OpMap.TryGetValue(bExpression.NodeType, out op)) throw new NotSupportedException();
            return $"{TranslateExpression(bExpression.Left)}[{op}]{TranslateExpression(bExpression.Right)}";
        }

        private object GetExpressionValue(object target, Expression exp)
        {
            if (exp == null)
            {
                return null;
            }

            switch (exp.NodeType)
            {
                case ExpressionType.Parameter:
                    return target;

                case ExpressionType.Constant:
                    return ((ConstantExpression)exp).Value;

                case ExpressionType.Lambda:
                    return exp;

                case ExpressionType.MemberAccess:
                    {
                        var memberExpression = (MemberExpression)exp;
                        if (!IsValidMemberExpression(memberExpression))
                        {
                            throw new NotSupportedException();
                        }

                        var parentValue = GetExpressionValue(target, memberExpression.Expression);
                        if (parentValue == null)
                        {
                            return null;
                        }

                        var propertyInfo = memberExpression.Member as PropertyInfo;
                        return propertyInfo != null
                            ? propertyInfo.GetValue(parentValue, null)
                            : ((FieldInfo)memberExpression.Member).GetValue(parentValue);
                    }
                case ExpressionType.Call:
                    {
                        var methodCallExpression = (MethodCallExpression)exp;
                        var parentValue = GetExpressionValue(target, methodCallExpression.Object);
                        if (parentValue == null && !methodCallExpression.Method.IsStatic)
                        {
                            return null;
                        }

                        var arguments = methodCallExpression.Arguments.Select(a => GetExpressionValue(target, a)).ToArray();

                        // Required for converting expression parameters to delegate calls
                        var parameters = methodCallExpression.Method.GetParameters();
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            if (typeof(Delegate).GetTypeInfo().IsAssignableFrom(parameters[i].ParameterType.GetTypeInfo()))
                            {
                                arguments[i] = ((LambdaExpression)arguments[i]).Compile();
                            }
                        }

                        if (arguments.Length > 0
                            && arguments[0] == null
                            && methodCallExpression.Method.IsStatic
                            && methodCallExpression.Method.IsDefined(typeof(ExtensionAttribute), false)) // extension method
                        {
                            return null;
                        }

                        return methodCallExpression.Method.Invoke(parentValue, arguments);
                    }
                case ExpressionType.Convert:
                    {
                        var unaryExpression = (UnaryExpression)exp;
                        return GetExpressionValue(target, unaryExpression.Operand);
                    }
            }

            throw new NotSupportedException();
        }

        private string GetValueLiteral(object value)
        {
            if (value == null) return "NULL";
            if (value is DateTime) return $"'{value:O}'";
            if (value is string || value is char || value is Guid) return $"'{value}'";
            return JsonConvert.SerializeObject(value, JsonSettings);
        }
    }
}