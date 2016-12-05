using RedArrow.Jsorm.Client.Session;

namespace RedArrow.Jsorm.Client.Config
{
    public interface IFluentConfigurator
    {
        IRemoteCreator Remote();

        IModelConfigurator Models();

        SessionFactoryConfiguration BuildFactoryConfiguration();

        ISessionFactory BuildSessionFactory();
    }
}