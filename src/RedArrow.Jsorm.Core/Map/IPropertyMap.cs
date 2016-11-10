using RedArrow.Jsorm.Core.Session;

namespace RedArrow.Jsorm.Core.Map
{
    public interface IPropertyMap
    {
        void Configure(ISessionFactory factory);
    }
}