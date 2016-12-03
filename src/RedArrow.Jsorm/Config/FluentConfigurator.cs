using RedArrow.Jsorm.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RedArrow.Jsorm.Config
{
    public class FluentConfigurator :
        IFluentConfigurator,
		IModelConfigurator,
        IRemoteCreator,
        IRemoteConfigure
    {
        private IList<Action<ModelLocator>> ModelConfigurators { get; }

        private Func<HttpClient> ClientCreator { get; set; }
        private IList<Action<HttpClient>> ClientConfigurators { get; }
		private IList<Func<HttpClient, Task>> AsyncClientConfigurators { get; }

        private SessionFactoryConfiguration SessionFactoryConfiguration { get; }

        internal FluentConfigurator()
            : this(new SessionFactoryConfiguration()) { }

        internal FluentConfigurator(SessionFactoryConfiguration config)
        {
            ModelConfigurators = new List<Action<ModelLocator>>();
            ClientConfigurators = new List<Action<HttpClient>>();
            AsyncClientConfigurators = new List<Func<HttpClient, Task>>();

            SessionFactoryConfiguration = config;
        }

        public IModelConfigurator Models()
        {
            return this;
		}

		public IModelConfigurator Configure(Action<ModelLocator> configureModel)
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

        public IRemoteConfigure ConfigureAsync(Func<HttpClient, Task> configureClient)
        {
            AsyncClientConfigurators.Add(configureClient);
            return this;
        }

        public SessionFactoryConfiguration BuildFactoryConfiguration()
        {
            // load all the models
            var modelConfig = new ModelLocator();
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

                client
                    .DefaultRequestHeaders
                    .Accept
                    .Add(MediaTypeWithQualityHeaderValue.Parse("application/vnd.api+json"));
                
                foreach (var configure in ClientConfigurators)
                {
                    configure(client);
                }

                Task.WaitAll(AsyncClientConfigurators.Select(x => x.Invoke(client)).ToArray());

                return client;
            };

            return SessionFactoryConfiguration;
        }

        public ISessionFactory BuildSessionFactory()
        {
            return BuildFactoryConfiguration()
                .BuildSessionFactory();
        }
    }
}