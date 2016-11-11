using RedArrow.Jsorm.Core.Map.Id.Generator;
using System;

namespace RedArrow.Jsorm.Core.Session
{
    public interface ISessionFactory
    {
        ISession CreateSession();

        void Register(Type modelType);

        void Register<TModel>(Func<object, Guid> getId);

        void Register<TModel>(IIdentifierGenerator generator);
    }
}