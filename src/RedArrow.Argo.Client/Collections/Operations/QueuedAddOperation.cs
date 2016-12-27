using System;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.JsonModels;
using RedArrow.Argo.Client.Session.Patch;
using RedArrow.Argo.Client.Session.Registry;

namespace RedArrow.Argo.Client.Collections.Operations
{
    public class QueuedAddOperation : AbstractQueuedOperation
    {
        private IModelRegistry ModelRegistry { get; }
        private object Item { get; }

        public QueuedAddOperation(
            IModelRegistry modelRegistry,
            string rltnName,
            object item) :
            base(rltnName)
        {
            ModelRegistry = modelRegistry;
            Item = item;
        }

        public override void Patch(PatchContext patchContext)
        {
            var id = ModelRegistry.GetModelId(Item);
            var resourceType = ModelRegistry.GetResourceType(Item.GetType());

            GetRelationshipData(patchContext)
                .Add(JToken.FromObject(new ResourceIdentifier {Id = id, Type = resourceType}));
        }
    }
}
