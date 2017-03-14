using System;
using System.Linq;
using System.Linq.Expressions;
using RedArrow.Argo.Client.Session;
using RedArrow.Argo.Linq.Queryables;

namespace RedArrow.Argo.Linq.Executors
{
	internal enum SingularExecutorType
	{
		First,
		Single,
		Last
	}

	internal class SingularExecutor<TModel> : RemoteExecutor<TModel>
	{
		private SingularExecutorType Type { get; }
		private bool IsOrDefault { get; }

		public SingularExecutor(
			RemoteQueryable<TModel> target,
			Expression<Func<TModel, bool>> expression,
			SingularExecutorType type,
			bool isOrDefault) :
			base(target, expression)
		{
			Type = type;
			IsOrDefault = isOrDefault;
		}

		public override TResult Execute<TResult>(IQuerySession session)
		{
			if (typeof (TResult) != typeof (TModel)) throw new NotSupportedException();

		    var targetQueryable = Target;
		    if (Expression != null)
		    {
		        targetQueryable = new WhereQueryable<TModel>(session, Target, Expression);
		    }

			var query = targetQueryable.BuildQuery();

			var results = session.Query<TResult>(query)
				.GetAwaiter()
				.GetResult();

			switch (Type)
			{
				case SingularExecutorType.First:
				{
					return IsOrDefault
						? results.FirstOrDefault()
						: results.First();
				}
				case SingularExecutorType.Single:
				{
					return IsOrDefault
						? results.SingleOrDefault()
						: results.Single();
				}
				case SingularExecutorType.Last:
				{
					return IsOrDefault
						? results.LastOrDefault()
						: results.Last();
				}
				default: // not sure how this is possible, but it makes the compiler happy
				{
					throw new NotSupportedException();
				}
			}
		}
	}
}
