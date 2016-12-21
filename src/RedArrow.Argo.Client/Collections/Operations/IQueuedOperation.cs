using RedArrow.Argo.Client.Session.Patch;

namespace RedArrow.Argo.Client.Collections.Operations
{
    public interface IQueuedOperation
    {
        void Patch(PatchContext patchContext);
    }
}
