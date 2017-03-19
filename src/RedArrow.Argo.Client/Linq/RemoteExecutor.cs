using System;
using System.Linq.Expressions;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Linq
{
	internal abstract class RemoteExecutor
	{
		public abstract TResult Execute<TResult>(IQuerySession session);
	}

	internal abstract class RemoteExecutor<TModel> : RemoteExecutor
	{
		protected RemoteQueryable<TModel> Target { get; }
		protected Expression<Func<TModel, bool>> Expression { get; }

		protected RemoteExecutor(
			RemoteQueryable<TModel> target,
			Expression<Func<TModel, bool>> expression)
		{
			Target = target;
			Expression = expression;
		}
	}
}
