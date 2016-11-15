using System;
using System.Reflection;

namespace RedArrow.Jsorm.Session
{
    public interface ISessionFactory
    {
        ISession CreateSession();
    }
}