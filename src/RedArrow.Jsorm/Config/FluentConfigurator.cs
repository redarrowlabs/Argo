using System;
using System.Collections.Generic;
using System.Net.Http;
using RedArrow.Jsorm.Cache;
using RedArrow.Jsorm.Session;

namespace RedArrow.Jsorm.Config
{
    public class FluentConfigurator
    {
        private IList<Action<ModelConfiguration>> ModelBuilders { get; }
		private IList<Action<HttpClient>>  ClientBuilders { get; }

        private Func<ICacheProvider> ModelRegistryBuilder { get; set; }

		internal SessionConfiguration SessionConfiguration { get; }

        public FluentConfigurator()
            : this(new SessionConfiguration()) { }

        public FluentConfigurator(SessionConfiguration config)
        {
            ModelBuilders = new List<Action<ModelConfiguration>>();
			ClientBuilders = new List<Action<HttpClient>>();
            SessionConfiguration = config;
        }

        public FluentConfigurator Models(Action<ModelConfiguration> mappings)
        {
            ModelBuilders.Add(mappings);
            return this;
        }

        //public FluentConfigurator Cache(Func<ICacheProvider> modelRegistry)
        //{
        //    ModelRegistryBuilder = modelRegistry;
        //    return this;
        //}

	    public FluentConfigurator Host(Action<HttpClient> configureClient)
	    {
			ClientBuilders.Add(configureClient);
		    return this;
	    }

	    public SessionConfiguration BuildConfiguration()
        {
			// load all the models
            var modelConfig = new ModelConfiguration();
            foreach (var builder in ModelBuilders)
            {
                builder(modelConfig);
            }

			// translate model attributes to session config
            modelConfig.Configure(SessionConfiguration);

			// build HttpClient factory
		    SessionConfiguration.HttpClientFactory = () =>
		    {
				var client = new HttpClient();
			    foreach (var clientBuilder in ClientBuilders)
			    {
				    clientBuilder(client);
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