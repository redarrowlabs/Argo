using RedArrow.Jsorm.Client.Collections;

namespace RedArrow.Jsorm.Client.Session
{
    public interface ICollectionSession
    {
        void InitializeCollection(IRemoteCollection collection);
    }
}
