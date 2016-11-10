namespace RedArrow.Jsorm.Core.Config
{
    public static class Jsorm
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