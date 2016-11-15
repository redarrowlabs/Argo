using RedArrow.Jsorm.Session;
using System;

namespace RedArrow.Jsorm.Config
{
    public interface IFluentConfigurator
    {
        IRemoteCreator Remote();

        IFluentConfigurator Models(Action<ModelConfiguration> mappings);

        SessionConfiguration BuildConfiguration();

        ISessionFactory BuildSessionFactory();
    }
}