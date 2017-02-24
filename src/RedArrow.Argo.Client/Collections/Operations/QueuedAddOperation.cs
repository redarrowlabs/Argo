using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Session.Patch;

namespace RedArrow.Argo.Client.Collections.Operations
{
    public class QueuedAddOperation : AbstractQueuedOperation
    {
        private Guid ItemId { get; }
        private string ItemResourceType { get; }

        public QueuedAddOperation(
            string rltnName,
            Guid itemId,
            string itemResourceType) :
            base(rltnName)
        {
            if(itemId == Guid.Empty) throw new ArgumentException("TODO", nameof(itemId));
            if(string.IsNullOrEmpty(itemResourceType)) throw new ArgumentNullException(nameof(itemResourceType));

            ItemId = itemId;
            ItemResourceType = itemResourceType;
        }

        public override void Patch(PatchContext patchContext)
        {
            var rltnData = GetRelationshipData(patchContext);
            if (!rltnData.SelectTokens($"$.[?(@.id=='{ItemId}')]").Any())
            {
                rltnData.Add(JToken.FromObject(new ResourceIdentifier {Id = ItemId, Type = ItemResourceType}));
            }
        }
    }
}
