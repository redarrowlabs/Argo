using System;
using RedArrow.Jsorm.Map.Id.Generator;

namespace RedArrow.Jsorm.Session
{
    public interface ISessionFactory
    {
        ISession CreateSession();

        void Register(Type modelType);

        void Register<TModel>(Func<object, Guid> getId);

        void Register<TModel>(IIdentifierGenerator generator);
    }
}