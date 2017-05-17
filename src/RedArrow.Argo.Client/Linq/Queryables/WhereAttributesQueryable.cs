using Newtonsoft.Json;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RedArrow.Argo.Client.Linq.Queryables
{
    internal class WhereAttributesQueryable<TModel> : WhereQueryable<TModel>
    {
        public WhereAttributesQueryable(
            IQuerySession session,
            RemoteQueryable<TModel> target,
            Expression<Func<TModel, bool>> predicate,
            JsonSerializerSettings jsonSettings)
            : base(session, target, predicate, jsonSettings)
        {
        }

        public override Func<MemberExpression, bool> IsValidMemberExpression => mExpression =>
            !mExpression.Member.CustomAttributes.Any(x => x.AttributeType == typeof(MetaAttribute));

        public override IQueryContext BuildQuery()
        {
            var query = Target.BuildQuery();

            var resourceType = typeof(TModel).GetModelResourceType();

            query.AppendAttributesFilter(resourceType, TranslateExpression(Predicate?.Body));

            return query;
        }
    }
}
