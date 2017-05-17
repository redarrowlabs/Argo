using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RedArrow.Argo.Client.Session
{
    public static class IQueryableExtensions
    {
        private static readonly MethodInfo QueryableMetaMethod = FindQueryableMetaMethod();

        public static IQueryable<TModel> Meta<TModel>(this IQueryable<TModel> queryable, Expression<Func<TModel, bool>> predicate)
        {
            return queryable.Provider.CreateQuery<TModel>(
                Expression.Call(
                    null,
                    QueryableMetaMethod.MakeGenericMethod(typeof(TModel)),
                    new[] { queryable.Expression, Expression.Quote(predicate) }
                )
            );
        }

        private static MethodInfo FindQueryableMetaMethod()
        {
            Expression<Func<IQueryable<object>>> meta = () => default(IQueryable<object>).Meta(default(Expression<Func<object, bool>>));
            return ((MethodCallExpression)meta.Body).Method.GetGenericMethodDefinition();
        }
    }
}
