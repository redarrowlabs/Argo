namespace RedArrow.Jsorm.Config
{
    public static class Fluently
    {
        public static IFluentConfigurator Configure()
        {
            return new FluentConfigurator();
        }

        public static IFluentConfigurator Configure(SessionFactoryConfiguration configuration)
        {
            return new FluentConfigurator(configuration);
        }
    }
}