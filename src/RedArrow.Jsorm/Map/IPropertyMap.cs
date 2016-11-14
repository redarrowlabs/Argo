using RedArrow.Jsorm.Session;

namespace RedArrow.Jsorm.Map
{
    public interface IPropertyMap
    {
        void Configure(SessionFactory factory);
    }
}