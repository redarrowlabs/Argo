using System;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Flurl.Shared;

namespace RedArrow.Argo.Client.Query
{
    public class RelationshipQueryContext<TParent, TModel> : QueryContext<TModel>
    {
        public RelationshipQueryContext(
            Guid id,
            string rltnName)
        {
            BasePath = typeof(TParent)
                .GetModelResourceType()
                .AppendPathSegments(id, rltnName);
        }
    }
}