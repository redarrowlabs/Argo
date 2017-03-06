using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Linq
{
	internal class RemoteQueryable<TIn, TOut> : IQueryable<TOut>
	{
		public Type ElementType => typeof (TOut);
		public Expression Expression { get; }
		public IQueryProvider Provider { get; }

		private ISession Session { get; }

		public RemoteQueryable(ISession session)
		{
			Session = session;
			Expression = Expression.Constant(this);
			Provider = new RemoteQueryProvider<TIn>(Session);
		}

		public RemoteQueryable(ISession session, RemoteQueryProvider<TIn> provider, Expression expression)
		{
			if(session == null) throw new ArgumentNullException(nameof(session));
			if(provider == null) throw new ArgumentNullException(nameof(provider));
			if(expression == null)throw new ArgumentNullException(nameof(expression));

			if(!typeof(IQueryable<TOut>).GetTypeInfo().IsAssignableFrom(expression.Type.GetTypeInfo()))
				throw new ArgumentOutOfRangeException(nameof(expression));

			Session = session;
			Provider = provider;
			Expression = expression;

		}

		public IEnumerator<TOut> GetEnumerator()
		{

		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
