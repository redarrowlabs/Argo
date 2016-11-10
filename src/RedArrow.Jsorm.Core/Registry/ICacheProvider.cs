using System;

namespace RedArrow.Jsorm.Core.Registry
{
    internal interface ICacheProvider
    {
        void Register(Type type);

        T Get<T>(object id);
    }
}