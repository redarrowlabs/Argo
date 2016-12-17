using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Config
{
    public interface IFluentConfigurator
    {
        IRemoteCreator Remote();

        IModelConfigurator Models();

        SessionFactoryConfiguration BuildFactoryConfiguration();

        ISessionFactory BuildSessionFactory();
    }
}