using RedArrow.Argo.Client.Collections;

namespace RedArrow.Argo.Client.Session
{
    internal interface ICollectionSession
    {
        void InitializeCollection<T>(IRemoteCollection<T> collection)
            where T : class;
    }
}