using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.JsonModels;
using RedArrow.Argo.Client.Session.Patch;
using RedArrow.Argo.Client.Session.Registry;

namespace RedArrow.Argo.Client.Collections.Operations
{
    public class QueuedRemoveOperation : AbstractQueuedOperation
    {
        private IModelRegistry ModelRegistry { get; }
        private object Item { get; }

        public QueuedRemoveOperation(
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
            GetRelationshipData(patchContext)
                .SelectToken($"$.[?(@.id=='{id}')]")
                ?.Remove();
        }
    }
}
