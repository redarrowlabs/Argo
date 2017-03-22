﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Config.Pipeline;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Config
{
    public class FluentConfigurator :
        IFluentConfigurator,
        IModelConfigurator,
        IRemoteConfigurator
    {
        private IList<Action<ModelScanner>> ModelScans { get; }
		
        private IList<Action<HttpClient>> ClientConfigurators { get; }
        private IList<Func<HttpClient, Task>> AsyncClientConfigurators { get; }
		
		private Action<IHttpClientBuilder> HttpClientBuilder { get; set; }
		
		private SessionFactoryConfiguration SessionFactoryConfiguration { get; }

        private Uri ApiHost { get; }

        internal FluentConfigurator(string apiHost)
            : this(apiHost, new SessionFactoryConfiguration()) { }

        internal FluentConfigurator(string apiHost, SessionFactoryConfiguration config)
        {
            ModelScans = new List<Action<ModelScanner>>();
            ClientConfigurators = new List<Action<HttpClient>>();
            AsyncClientConfigurators = new List<Func<HttpClient, Task>>();
			
			ApiHost = new Uri(apiHost);

            SessionFactoryConfiguration = config;
        }

        public IModelConfigurator Models()
        {
            return this;
        }

        public IModelConfigurator Configure(Action<ModelScanner> scan)
        {
            ModelScans.Add(scan);
            return this;
        }

        public IRemoteConfigurator Remote()
        {
            return this;
        }

        public IRemoteConfigurator Configure(Action<HttpClient> configureClient)
        {
            ClientConfigurators.Add(configureClient);
            return this;
        }

        public IRemoteConfigurator ConfigureAsync(Func<HttpClient, Task> configureClient)
        {
            AsyncClientConfigurators.Add(configureClient);
            return this;
        }

	    public IRemoteConfigurator Configure(Action<IHttpClientBuilder> builder)
	    {
		    HttpClientBuilder = builder;
		    return this;
	    }

        public SessionFactoryConfiguration BuildFactoryConfiguration()
        {
            // load all the models
            var modelScanner = new ModelScanner();
            foreach (var scan in ModelScans)
            {
                scan(modelScanner);
            }

            // translate model attributes to session config
            modelScanner.Configure(SessionFactoryConfiguration);

            ClientConfigurators.Add(client => client.BaseAddress = ApiHost);

	        var builder = new HttpClientBuilder();
	        (HttpClientBuilder ?? (_ => { }))(builder);
	        
			// build HttpClient factory
			SessionFactoryConfiguration.HttpClientFactory = () =>
            {
				var client = new HttpClient(builder.Build());

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
            return BuildFactoryConfiguration().BuildSessionFactory();
        }
    }
}