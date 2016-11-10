using RedArrow.Jsorm.Core.Session;

namespace RedArrow.Jsorm.Core.Map
{
    public interface IResourceMap
    {
        void Configure(ISessionFactory factory);
    }
}