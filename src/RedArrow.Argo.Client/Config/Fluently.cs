namespace RedArrow.Argo.Client.Config
{
    public static class Fluently
    {
        public static IFluentConfigurator Configure(string apiHost)
        {
            return new FluentConfigurator(apiHost);
        }

        public static IFluentConfigurator Configure(string apiHost, SessionFactoryConfiguration configuration)
        {
            return new FluentConfigurator(apiHost, configuration);
        }
    }
}