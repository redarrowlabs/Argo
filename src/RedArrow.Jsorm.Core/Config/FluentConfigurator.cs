using RedArrow.Jsorm.Core.Registry;
using RedArrow.Jsorm.Core.Session;
using System;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Core.Config
{
    public class FluentConfigurator
    {
        private IList<Action<MappingConfiguration>> MapBuilders { get; }
        private Func<IModelRegistry> ModelRegistryBuilder { get; set; }

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

        public FluentConfigurator Registry(Func<AbstractModelRegistry> modelRegistry)
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

            SessionConfiguration.ModelRegistry = ModelRegistryBuilder();

            return SessionConfiguration;
        }

        public ISessionFactory BuildSessionFactory()
        {
            return BuildConfiguration()
                .BuildSessionFactory();
        }
    }
}