using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Linq
{
	internal class RemoteQueryProvider<TIn> : IQueryProvider
	{
		private ISession Session { get; }

		public RemoteQueryProvider(ISession session)
		{
			Session = session;
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			return new RemoteQueryable<TIn, TElement>(Session, this, expression);
		}

		public TResult Execute<TResult>(Expression expression)
		{
			throw new NotImplementedException();
		}

		public IQueryable CreateQuery(Expression expression)
		{
			throw new NotImplementedException();
		}

		public object Execute(Expression expression)
		{
			throw new NotImplementedException();
		}
	}
}
