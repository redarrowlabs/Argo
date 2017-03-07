using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RedArrow.Argo.Linq
{
	internal class RemoteQueryProvider : IQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetElementType();
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(RemoteQueryable<>).MakeGenericType(elementType), this, expression);
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        // Queryable's collection-returning standard query operators call this method.
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
            return new RemoteQueryable<TElement>(this, expression);
		}


		public TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }

        // Queryable's "single value" standard query operators call this method.
        // It is also called from .GetEnumerator(). 
        public object Execute(Expression expression)
		{
			throw new NotImplementedException();
		}
	}
}
