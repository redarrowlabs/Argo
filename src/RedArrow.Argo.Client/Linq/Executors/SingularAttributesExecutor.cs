using System;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using RedArrow.Argo.Client.Linq.Queryables;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Linq.Executors
{
    internal enum SingularExecutorType
    {
        First,
        Single,
        Last
    }

    internal class SingularAttributesExecutor<TModel> : RemoteExecutor<TModel>
    {
        private SingularExecutorType Type { get; }
        private bool IsOrDefault { get; }

        private JsonSerializerSettings JsonSettings { get; }

        public SingularAttributesExecutor(
            RemoteQueryable<TModel> target,
            Expression<Func<TModel, bool>> expression,
            JsonSerializerSettings jsonSettings,
            SingularExecutorType type,
            bool isOrDefault) :
            base(target, expression)
        {
            Type = type;
            IsOrDefault = isOrDefault;
            JsonSettings = jsonSettings;
        }

        public override TResult Execute<TResult>(IQuerySession session)
        {
            if (typeof(TResult) != typeof(TModel)) throw new NotSupportedException();

            var targetQueryable = Target;
            if (Expression != null)
            {
                targetQueryable = new WhereAttributesQueryable<TModel>(session, Target, Expression, JsonSettings);
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