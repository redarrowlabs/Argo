using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RedArrow.Argo.Linq.Node;

namespace RedArrow.Argo.Linq.Extensions
{
    internal static class ExpressionExtensions
    {
        private static readonly IDictionary<string, Func<MethodCallExpression, NodeBase>> ParseMap =
            new Dictionary<string, Func<MethodCallExpression, NodeBase>>
            {
                //{"Where", ParseWhere},
                //{"Select", ParseSelect},
                //{"SelectMany", ParseSelectMany},
                //{"Cast", ParseCast},
                //{"OfType", ParseOfType},
                //{"Distinct", ParseDistinct},
                //{"Skip", ParseSkip},
                //{"Take", ParseTake}
            };

        public static NodeBase Parse(this Expression expression)
        {
            if (expression is ConstantExpression) return NodeBase.DirectValue;

            if(!(expression is MethodCallExpression))
                throw new NotSupportedException();

            var methodCallExpression = (MethodCallExpression) expression;
            var methodName = methodCallExpression.Method.Name;

            Func<MethodCallExpression, NodeBase> parseMethod;
            if (ParseMap.TryGetValue(methodName, out parseMethod))
                return parseMethod(methodCallExpression);

            throw new NotSupportedException($"The method '{methodName}' is not supported.");
        }
    }
}
