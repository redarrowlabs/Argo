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
            // TODO: better jsonpath query here
            // according to jsonpath.com...
            // $.[?(@.id=="{id}" && @.type=="{type}")]
            var id = ModelRegistry.GetModelId(Item);
            GetRelationshipData(patchContext)
                .SingleOrDefault(x => x
                    .SelectToken("id")
                    ?.ToObject<Guid>() == id)
                ?.Remove();
        }
    }
}
