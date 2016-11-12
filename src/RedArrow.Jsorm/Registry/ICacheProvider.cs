using System;

namespace RedArrow.Jsorm.Registry
{
    public interface ICacheProvider
    {
        void Register(Type type);

        T Get<T>(object id);
    }
}