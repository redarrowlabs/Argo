using RedArrow.Jsorm.Session;
using System;

namespace RedArrow.Jsorm.Config
{
    public interface IFluentConfigurator
    {
        IRemoteCreator Remote();

        IModelConfigurator Models();

        SessionFactoryConfiguration BuildFactoryConfiguration();

        ISessionFactory BuildSessionFactory();
    }
}