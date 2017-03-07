using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RedArrow.Argo.Linq
{
	internal class RemoteQueryable<TModel> : IOrderedQueryable<TModel>
	{
		public Type ElementType => typeof (TModel);
		public Expression Expression { get; }
		public IQueryProvider Provider { get; }

		public RemoteQueryable()
        {
            Provider = new RemoteQueryProvider();
            Expression = Expression.Constant(this);
		}

		public RemoteQueryable(RemoteQueryProvider provider, Expression expression)
		{
			if(provider == null) throw new ArgumentNullException(nameof(provider));
			if(expression == null)throw new ArgumentNullException(nameof(expression));

			if(!typeof(IQueryable<TModel>).GetTypeInfo().IsAssignableFrom(expression.Type.GetTypeInfo()))
				throw new ArgumentOutOfRangeException(nameof(expression));
            
			Provider = provider;
			Expression = expression;

		}

		public IEnumerator<TModel> GetEnumerator()
		{
		    return Provider.Execute<IEnumerable<TModel>>(Expression).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
		    return Provider.Execute<IEnumerable>(Expression).GetEnumerator();
		}
	}
}
