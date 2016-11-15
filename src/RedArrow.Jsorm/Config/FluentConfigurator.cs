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

        private Func<ICacheProvider> ModelRegistryBuilder { get; set; }

        internal SessionConfiguration SessionConfiguration { get; }

        public FluentConfigurator()
            : this(new SessionConfiguration()) { }

        public FluentConfigurator(SessionConfiguration config)
        {
            ModelConfigurators = new List<Action<ModelConfiguration>>();

            ClientConfigurators = new List<Action<HttpClient>>();

            SessionConfiguration = config;
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

		//public FluentConfigurator Cache(Func<ICacheProvider> modelRegistry)
		//{
		//    ModelRegistryBuilder = modelRegistry;
		//    return this;
		//}

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

        public SessionConfiguration BuildConfiguration()
        {
            // load all the models
            var modelConfig = new ModelConfiguration();
            foreach (var builder in ModelConfigurators)
            {
                builder(modelConfig);
            }

            // translate model attributes to session config
            modelConfig.Configure(SessionConfiguration);

            // build HttpClient factory
            SessionConfiguration.HttpClientFactory = () =>
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
            //SessionConfiguration.CacheProvider = ModelRegistryBuilder();

            return SessionConfiguration;
        }

        public ISessionFactory BuildSessionFactory()
        {
            return BuildConfiguration()
                .BuildSessionFactory();
        }
    }
}