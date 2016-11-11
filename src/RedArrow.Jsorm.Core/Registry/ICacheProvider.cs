using System;

namespace RedArrow.Jsorm.Core.Registry
{
    public interface ICacheProvider
    {
        void Register(Type type);

        T Get<T>(object id);
    }
}