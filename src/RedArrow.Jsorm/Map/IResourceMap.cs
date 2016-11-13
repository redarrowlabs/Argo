using RedArrow.Jsorm.Session;

namespace RedArrow.Jsorm.Map
{
    public interface IResourceMap
    {
        void Configure(ISessionFactory factory);
    }
}