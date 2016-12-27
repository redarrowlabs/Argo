using RedArrow.Argo.Client.Collections;

namespace RedArrow.Argo.Client.Session
{
    internal interface ICollectionSession
    {
        void InitializeCollection(IRemoteCollection collection);
    }
}