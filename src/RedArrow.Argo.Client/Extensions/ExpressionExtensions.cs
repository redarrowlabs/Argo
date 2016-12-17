using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RedArrow.Argo.Client.Extensions
{
	public static class ExpressionExtensions
	{
		public static string PropertyName<TModel, TProp>(this Expression<Func<TModel, TProp>> expression)
		{
			var lambda = (LambdaExpression)expression;

			var body = lambda.Body as UnaryExpression;
			var memberExpression = body != null
				? (MemberExpression)body.Operand
				: (MemberExpression)lambda.Body;

			return ((PropertyInfo)memberExpression.Member).Name;
		}
	}
}
