using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Linq
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

		protected RemoteQueryable(IQuerySession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            Provider = new RemoteQueryProvider(session);
            Expression = Expression.Constant(this);
            Session = session;
        }

		protected RemoteQueryable(RemoteQueryProvider provider, Expression expression, IQuerySession session)
		{
			if(provider == null) throw new ArgumentNullException(nameof(provider));
			if(expression == null)throw new ArgumentNullException(nameof(expression));
		    if (session == null) throw new ArgumentNullException(nameof(session));

			if(!typeof(IQueryable<TModel>).GetTypeInfo().IsAssignableFrom(expression.Type.GetTypeInfo()))
				throw new ArgumentOutOfRangeException(nameof(expression));
            
			Provider = provider;
			Expression = expression;
		    Session = session;
		}

		public IEnumerator<TModel> GetEnumerator()
		{
		    return Session.Query<TModel>(BuildQuery())
		        .GetAwaiter()
		        .GetResult()
		        .GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
		    return GetEnumerator();
		}

	    public abstract QueryContext BuildQuery();

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
