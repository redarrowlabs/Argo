using RedArrow.Jsorm.Cache;
using RedArrow.Jsorm.Session;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RedArrow.Jsorm.Config
{
    public class FluentConfigurator :
        IFluentConfigurator,
		IModelConfigurator,
        IRemoteCreator,
        IRemoteConfigure
    {
        private IList<Action<ModelConfiguration>> ModelConfigurators { get; }

        private Func<HttpClient> ClientCreator { get; set; }
        private IList<Action<HttpClient>> ClientConfigurators { get; }
		
        internal SessionFactoryConfiguration SessionFactoryConfiguration { get; }

        public FluentConfigurator()
            : this(new SessionFactoryConfiguration()) { }

        public FluentConfigurator(SessionFactoryConfiguration config)
        {
            ModelConfigurators = new List<Action<ModelConfiguration>>();

            ClientConfigurators = new List<Action<HttpClient>>();

            SessionFactoryConfiguration = config;
        }

        public IModelConfigurator Models()
        {
            return this;
		}

		public IModelConfigurator Configure(Action<ModelConfiguration> configureModel)
		{
			ModelConfigurators.Add(configureModel);
			return this;
		}
		
		public IRemoteCreator Remote()
        {
            return this;
        }

        public IRemoteConfigure Create(Func<HttpClient> createClient)
        {
            ClientCreator = createClient;
            return this;
        }

        public IRemoteConfigure Configure(Action<HttpClient> configureClient)
        {
            ClientConfigurators.Add(configureClient);
            return this;
        }

        public SessionFactoryConfiguration BuildFactoryConfiguration()
        {
            // load all the models
            var modelConfig = new ModelConfiguration();
            foreach (var builder in ModelConfigurators)
            {
                builder(modelConfig);
            }

            // translate model attributes to session config
            modelConfig.Configure(SessionFactoryConfiguration);

            // build HttpClient factory
            SessionFactoryConfiguration.HttpClientFactory = () =>
            {
                var client = (ClientCreator ?? (() => new HttpClient()))();
                client.DefaultRequestHeaders
                    .Accept
                    .Add(MediaTypeWithQualityHeaderValue
                        .Parse("application/vnd.api+json"));
                foreach (var configure in ClientConfigurators)
                {
                    configure(client);
                }
                return client;
            };
            //SessionFactoryConfiguration.CacheProvider = ModelRegistryBuilder();

            return SessionFactoryConfiguration;
        }

        public ISessionFactory BuildSessionFactory()
        {
            return BuildFactoryConfiguration()
                .BuildSessionFactory();
        }
    }
}