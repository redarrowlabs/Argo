using System;

namespace RedArrow.Jsorm.Core.Session
{
    public interface ISessionFactory
    {
        ISession CreateSession();

        void Register(Func<object, object> getId);
    }
}