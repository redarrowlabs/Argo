using System;
using System.Linq;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Session.Patch;

namespace RedArrow.Argo.Client.Collections.Operations
{
    public class QueuedRemoveOperation : AbstractQueuedOperation
    {
        protected Guid ItemId { get; }

        public QueuedRemoveOperation(
            string rltnName,
            Guid itemId) :
            base(rltnName)
        {
            if(itemId == Guid.Empty) throw new ArgumentException("TODO", nameof(itemId));

            ItemId = itemId;
        }

        public override void Patch(PatchContext patchContext)
        {
            // it is slightly safer to do SelectTokens (plural) instead of 
            // SelectToken (singular) in case there is a problem with the data.
            // just remove all matches
            GetRelationshipData(patchContext)
                .SelectTokens($"$.[?(@.id=='{ItemId}')]")
                .ToArray()
                .Each(x => x.Remove());
        }
    }
}
