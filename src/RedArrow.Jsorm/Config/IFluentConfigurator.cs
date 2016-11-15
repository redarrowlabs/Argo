using RedArrow.Jsorm.Session;
using System;

namespace RedArrow.Jsorm.Config
{
    public interface IFluentConfigurator
    {
        IRemoteCreator Remote();

        IModelConfigurator Models();

        SessionConfiguration BuildConfiguration();

        ISessionFactory BuildSessionFactory();
    }
}