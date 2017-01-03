using RedArrow.Argo.Client.Session.Patch;

namespace RedArrow.Argo.Client.Collections.Operations
{
    public class QueuedClearOperation : AbstractQueuedOperation
    {
        public QueuedClearOperation(string rltnName) : base(rltnName)
        {
        }

        public override void Patch(PatchContext patchContext)
        {
            GetRelationshipData(patchContext).RemoveAll();
        }
    }
}
