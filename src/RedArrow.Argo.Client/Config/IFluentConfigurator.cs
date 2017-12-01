using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Config
{
    public interface IFluentConfigurator
    {
        IRemoteConfigurator Remote();

        IModelConfigurator Models();

        SessionFactoryConfiguration BuildFactoryConfiguration();

        ISessionFactory BuildSessionFactory();
    }
}