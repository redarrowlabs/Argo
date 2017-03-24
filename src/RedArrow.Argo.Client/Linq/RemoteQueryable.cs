using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Linq.Behaviors;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Linq
{
	internal abstract class RemoteQueryable<TModel> : IOrderedQueryable<TModel>
	{
        private static readonly ISet<Type> PropAttrTypes = new HashSet<Type>
        {
            typeof(PropertyAttribute),
            typeof(HasOneAttribute),
            typeof(HasManyAttribute)
        };

        protected IQuerySession Session { get; }

		public Type ElementType => typeof (TModel);
        public Expression Expression { get; }

	    public IQueryProvider Provider { get; }
		public IQueryBehavior Behavior { get; set; }

		protected RemoteQueryable(IQuerySession session, IQueryProvider provider, IQueryBehavior behavior)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            Expression = Expression.Constant(this);
            Provider = provider;
            Session = session;
	        Behavior = behavior;
        }

		protected RemoteQueryable(IQuerySession session, IQueryProvider provider, Expression expression)
		{
			if (provider == null) throw new ArgumentNullException(nameof(provider));
			if (expression == null) throw new ArgumentNullException(nameof(expression));
		    if (session == null) throw new ArgumentNullException(nameof(session));

			if (!typeof(IQueryable<TModel>).GetTypeInfo().IsAssignableFrom(expression.Type.GetTypeInfo()))
				throw new ArgumentOutOfRangeException(nameof(expression));
            
			Expression = expression;
            Provider = provider;
            Session = session;
		}

		public IEnumerator<TModel> GetEnumerator()
		{
			return Behavior.ExecuteQuery<TModel>(BuildQuery());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
		    return GetEnumerator();
		}

	    public abstract IQueryContext BuildQuery();

	    protected string GetJsonName(Type type)
        {
            return type.GetTypeInfo()
                .CustomAttributes
                .Single(a => a.AttributeType == typeof(ModelAttribute))
                .ConstructorArguments
                .Select(arg => arg.Value as string)
                .FirstOrDefault() ?? type.Name.Camelize();
        }

	    protected string GetJsonName(MemberInfo member)
	    {
	        return member.CustomAttributes
	            .Single(a => PropAttrTypes.Contains(a.AttributeType))
	            .ConstructorArguments
	            .Select(arg => arg.Value as string)
	            .FirstOrDefault() ?? member.Name.Camelize();
	    }
	}
}
