using System;

namespace RedArrow.Jsorm.Core.Session
{
    public interface ISessionFactory
    {
        void Register(Type modelType);
    }
}