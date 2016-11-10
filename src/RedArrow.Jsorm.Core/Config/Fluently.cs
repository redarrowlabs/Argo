namespace RedArrow.Jsorm.Core.Config
{
    public static class Fluently
    {
        public static FluentConfigurator Configure()
        {
            return new FluentConfigurator();
        }

        public static FluentConfigurator Configure(SessionConfiguration configuration)
        {
            return new FluentConfigurator(configuration);
        }
    }
}