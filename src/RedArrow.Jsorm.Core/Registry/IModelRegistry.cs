using System;

namespace RedArrow.Jsorm.Core.Registry
{
    internal interface IModelRegistry
    {
        void Register(Type type);

        T Resolve<T>();
    }
}