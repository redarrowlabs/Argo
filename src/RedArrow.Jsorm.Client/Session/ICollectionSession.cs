using RedArrow.Jsorm.Client.Collections;

namespace RedArrow.Jsorm.Client.Session
{
    internal interface ICollectionSession
    {

        void InitializeCollection<T>(IRemoteCollection<T> collection)
            where T : class;
    }
}
