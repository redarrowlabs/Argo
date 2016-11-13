using System;
using System.Collections.Generic;
using RedArrow.Jsorm.Registry;
using RedArrow.Jsorm.Session;

namespace RedArrow.Jsorm.Config
{
    public class FluentConfigurator
    {
        private IList<Action<MappingConfiguration>> MapBuilders { get; }
        private Func<ICacheProvider> ModelRegistryBuilder { get; set; }

        internal SessionConfiguration SessionConfiguration { get; }

        public FluentConfigurator()
            : this(new SessionConfiguration()) { }

        public FluentConfigurator(SessionConfiguration config)
        {
            MapBuilders = new List<Action<MappingConfiguration>>();
            SessionConfiguration = config;
        }

        public FluentConfigurator Mappings(Action<MappingConfiguration> mappings)
        {
            MapBuilders.Add(mappings);
            return this;
        }

        public FluentConfigurator Cache(Func<ICacheProvider> modelRegistry)
        {
            ModelRegistryBuilder = modelRegistry;
            return this;
        }

        public SessionConfiguration BuildConfiguration()
        {
            var mapConfig = new MappingConfiguration();

            foreach (var builder in MapBuilders)
            {
                builder(mapConfig);
            }

            mapConfig.Configure(SessionConfiguration);

            SessionConfiguration.CacheProvider = ModelRegistryBuilder();

            return SessionConfiguration;
        }

        public ISessionFactory BuildSessionFactory()
        {
            return BuildConfiguration()
                .BuildSessionFactory();
        }
    }
}