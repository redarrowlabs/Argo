using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RedArrow.Argo.Client.Session;
using RedArrow.Argo.Linq.Executors;
using RedArrow.Argo.Linq.Queryables;

namespace RedArrow.Argo.Linq
{
    internal class RemoteQueryProvider : IQueryProvider
    {
        private IQuerySession Session { get; }

        public RemoteQueryProvider(IQuerySession session)
        {
            Session = session;
        }

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
        public IQueryable<TModel> CreateQuery<TModel>(Expression expression)
        {
            return CreateQueryInternal<TModel>(expression);
        }
		
        // Queryable's "single value" standard query operators call this method.
        // It is also called from .GetEnumerator(). 
		// TResult may not be TModel.  may be bool (.Any()) or int (.Count())
        public TResult Execute<TResult>(Expression expression)
        {
	        return ExecuteInternal(expression).Execute<TResult>(Session);
        }

        // TODO: not sure how to support this since model type is not known?
        // TODO: can this be derrived from expression?
        public object Execute(Expression expression)
		{
			throw new NotImplementedException();
		}

        private RemoteQueryable<TModel> CreateQueryInternal<TModel>(Expression expression)
		{
			if (expression is ConstantExpression)
			{
				return new TypeQueryable<TModel>(Session, this);
			}

			var mcExpression = expression as MethodCallExpression;
			if (mcExpression == null) throw new NotSupportedException();

			if (mcExpression.Arguments.Count < 2) throw new NotSupportedException();

            var methodName = mcExpression.Method.Name;
            var target = mcExpression.Arguments[0];
            var operand = ((UnaryExpression)mcExpression.Arguments[1]).Operand;

			// TODO: there's probably a better way...
			// can't do a static string => func index due to type args
			// index would need to be built in ctor per instance: memory vs. fugly code ¯\_(ツ)_/¯
			switch (methodName)
            {
                case "Where":
                {
                    return CreateWhereQuery<TModel>(target, operand);
                }
                case "OrderBy":
                {
                    return CreateOrderByQuery<TModel>(target, operand, false);
                }
                case "OrderByDescending":
                {
                    return CreateOrderByQuery<TModel>(target, operand, true);
                }
                case "ThenBy":
                {
					return CreateOrderByQuery<TModel>(target, operand, false);
                }
                case "ThenByDescending":
                {
                    return CreateOrderByQuery<TModel>(target, operand, true);
                }
                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

	    private RemoteExecutor ExecuteInternal(Expression expression)
	    {
		    var mcExpression = expression as MethodCallExpression;
		    if (mcExpression == null) throw new NotSupportedException();

		    if (mcExpression.Arguments.Count == 0) throw new NotSupportedException();

		    var methodName = mcExpression.Method.Name;
		    var targetExpression = mcExpression.Arguments[0];

		    Type targetType;
		    object target;
		    GetExecuteTarget(targetExpression, out target, out targetType);

		    var predicate = mcExpression.Arguments.Count > 1
			    ? ((UnaryExpression) mcExpression.Arguments[1]).Operand
			    : null;

		    switch (methodName)
		    {
			    case "First":
			    {
				    return (RemoteExecutor) GetExecutorCtor(typeof (SingularExecutor<>), targetType)
					    .Invoke(new[] {target, predicate, SingularExecutorType.First, false});
			    }
			    case "FirstOrDefault":
			    {
				    return (RemoteExecutor) GetExecutorCtor(typeof (SingularExecutor<>), targetType)
					    .Invoke(new[] {target, predicate, SingularExecutorType.First, true});
			    }
			    case "Single":
			    {
				    return (RemoteExecutor) GetExecutorCtor(typeof (SingularExecutor<>), targetType)
					    .Invoke(new[] {target, predicate, SingularExecutorType.Single, false});
			    }
			    case "SingleOrDefault":
			    {
				    return (RemoteExecutor) GetExecutorCtor(typeof (SingularExecutor<>), targetType)
					    .Invoke(new[] {target, predicate, SingularExecutorType.Single, true});
			    }
			    case "Last":
			    {
				    return (RemoteExecutor) GetExecutorCtor(typeof (SingularExecutor<>), targetType)
					    .Invoke(new[] {target, predicate, SingularExecutorType.Last, false});
			    }
			    case "LastOrDefault":
			    {
				    return (RemoteExecutor) GetExecutorCtor(typeof (SingularExecutor<>), targetType)
					    .Invoke(new[] {target, predicate, SingularExecutorType.Last, true});
			    }
			    default:
			    {
				    throw new NotSupportedException();
			    }
		    }
	    }

        private RemoteQueryable<TModel> CreateWhereQuery<TModel>(Expression target, Expression operand)
        {
            var targetQueryable = CreateQueryInternal<TModel>(target);

            return new WhereQueryable<TModel>(
                Session,
                targetQueryable,
                operand as Expression<Func<TModel, bool>>);
        }

	    private RemoteQueryable<TModel> CreateOrderByQuery<TModel>(Expression target, Expression operand, bool isDesc)
        {
            var targetQueryable = CreateQueryInternal<TModel>(target);

            var memberType = operand.GetType().GetTypeInfo()
                .GenericTypeArguments[0]
                .GenericTypeArguments[1];

            var genericOrderyBy = typeof(OrderByQueryable<,>).MakeGenericType(typeof(TModel), memberType);
            var ctor = genericOrderyBy.GetTypeInfo().DeclaredConstructors.Single();

            return (RemoteQueryable<TModel>) ctor.Invoke(new object[]
            {
                Session,
                targetQueryable,
                operand,
                isDesc
            });
        }

	    private void GetExecuteTarget(Expression targetExpression, out object target, out Type targetType)
	    {
		    var cExpression = targetExpression as ConstantExpression;
		    if (cExpression != null)
		    {
			    target = cExpression.Value;
			    targetType = target.GetType().GenericTypeArguments[0];
			    return;
		    }

		    var mcExpression = targetExpression as MethodCallExpression;
			if(mcExpression == null) throw new NotSupportedException();

		    targetType = mcExpression.Method.ReturnType;
		    targetType = targetType.GenericTypeArguments[0];
		    target = GetType().GetTypeInfo().DeclaredMethods
			    .Single(x => x.IsGenericMethod && x.Name == "CreateQuery")
				.MakeGenericMethod(targetType)
				.Invoke(this, new object[] {targetExpression});
	    }

		private static ConstructorInfo GetExecutorCtor(Type executorType, Type resultType)
		{
			return executorType.MakeGenericType(resultType)
				.GetTypeInfo()
				.DeclaredConstructors
				.Single();
		}
    }
}
