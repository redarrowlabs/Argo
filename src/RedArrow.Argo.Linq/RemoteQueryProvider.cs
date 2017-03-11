using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RedArrow.Argo.Client.Session;
using RedArrow.Argo.Linq.Methods;

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

        private RemoteQueryable<TModel> CreateQueryInternal<TModel>(Expression expression)
        {
            if (expression is ConstantExpression)
            {
                return new TypeQueryable<TModel>(Session);
            }

            var methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression == null) throw new NotSupportedException();

            return CreateQuery<TModel>(methodCallExpression);
        }

        // Queryable's "single value" standard query operators call this method.
        // It is also called from .GetEnumerator(). 
        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }

        // TODO: not sure how to support this since model type is not known?
        // TODO: can this be derrived from expression?
        public object Execute(Expression expression)
		{
			throw new NotImplementedException();
		}

        private RemoteQueryable<TModel> CreateQuery<TModel>(MethodCallExpression expression)
        {
            if (expression.Arguments.Count < 2) throw new NotSupportedException();

            var methodName = expression.Method.Name;
            var target = expression.Arguments[0];
            var operand = ((UnaryExpression) expression.Arguments[1]).Operand;

            switch (methodName)
            {
                case "OrderBy":
                {
                    return CreateOrderByQuery<TModel>(target, operand, false, false);
                }
                case "OrderByDescending":
                {
                    return CreateOrderByQuery<TModel>(target, operand, true, false);
                }
                case "ThenBy":
                {
                    //return CreateOrderByQuery<TModel>(target, operand, false, true);
                    throw new NotSupportedException();
                }
                case "ThenByDescending":
                {
                    //return CreateOrderByQuery<TModel>(target, operand, true, true);
                    throw new NotSupportedException();
                }
                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        private RemoteQueryable<TModel> CreateOrderByQuery<TModel>(Expression target, Expression operand, bool isDesc, bool isThenBy)
        {
            var targetQueryable = CreateQueryInternal<TModel>(target);

            var memberType = operand.GetType().GetTypeInfo()
                .GenericTypeArguments[0]
                .GenericTypeArguments[1];

            var genericOrderyBy = typeof(OrderByQueryable<,>).MakeGenericType(typeof(TModel), memberType);
            var ctor = genericOrderyBy.GetTypeInfo().DeclaredConstructors.Single();

            return (RemoteQueryable<TModel>) ctor.Invoke(new object[]
            {
                targetQueryable,
                operand,
                Session,
                isDesc,
                isThenBy
            });
        }
    }
}
