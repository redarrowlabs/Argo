﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Http;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Config
{
    public class FluentConfigurator :
        IFluentConfigurator,
        IModelConfigurator,
        IRemoteCreator,
        IRemoteConfigure
    {
        private IList<Action<ModelScanner>> ModelScans { get; }

        private Func<HttpClient> ClientCreator { get; set; }
        private IList<Action<HttpClient>> ClientConfigurators { get; }
        private IList<Func<HttpClient, Task>> AsyncClientConfigurators { get; }

	    private bool HasDevDefinedCallbacks =>
		    HttpResponseMessageCallbacks.Any() ||
		    ResourceCreatedCallbacks.Any() ||
		    ResourceUpdatedCallbacks.Any() ||
		    ResourceRetrievedCallbacks.Any() ||
		    ResourceDeletedCallbacks.Any();

		private IList<Func<HttpResponseMessage, Task>> HttpResponseMessageCallbacks { get; }
		private IList<Func<HttpResponseMessage, Task>> ResourceCreatedCallbacks { get; }
		private IList<Func<HttpResponseMessage, Task>> ResourceUpdatedCallbacks { get; }
		private IList<Func<HttpResponseMessage, Task>> ResourceRetrievedCallbacks { get; }
		private IList<Func<HttpResponseMessage, Task>> ResourceDeletedCallbacks { get; }

		private SessionFactoryConfiguration SessionFactoryConfiguration { get; }

        private Uri ApiHost { get; }
        private Func<HttpMessageHandler> CreateHttpMessageHandler { get; set; }

        internal FluentConfigurator(string apiHost)
            : this(apiHost, new SessionFactoryConfiguration()) { }

        internal FluentConfigurator(string apiHost, SessionFactoryConfiguration config)
        {
            ModelScans = new List<Action<ModelScanner>>();
            ClientConfigurators = new List<Action<HttpClient>>();
            AsyncClientConfigurators = new List<Func<HttpClient, Task>>();

			HttpResponseMessageCallbacks = new List<Func<HttpResponseMessage, Task>>();
			ResourceCreatedCallbacks = new List<Func<HttpResponseMessage, Task>>();
			ResourceUpdatedCallbacks = new List<Func<HttpResponseMessage, Task>>();
			ResourceRetrievedCallbacks = new List<Func<HttpResponseMessage, Task>>();
			ResourceDeletedCallbacks = new List<Func<HttpResponseMessage, Task>>();

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

        public IRemoteConfigure UseMessageHandler(Func<HttpMessageHandler> createHandler)
        {
            CreateHttpMessageHandler = createHandler;
            return this;
        }

	    public IRemoteConfigure OnHttpResponse(Func<HttpResponseMessage, Task> responseReceived)
	    {
		    HttpResponseMessageCallbacks.Add(responseReceived);
		    return this;
	    }

	    public IRemoteConfigure OnResourceCreated(Func<HttpResponseMessage, Task> resourceCreated)
	    {
		    ResourceCreatedCallbacks.Add(resourceCreated);
		    return this;
	    }

	    public IRemoteConfigure OnResourceUpdated(Func<HttpResponseMessage, Task> resourceUpdated)
	    {
		    ResourceUpdatedCallbacks.Add(resourceUpdated);
		    return this;
	    }

	    public IRemoteConfigure OnResourceRetreived(Func<HttpResponseMessage, Task> resourceRetrieved)
	    {
		    ResourceRetrievedCallbacks.Add(resourceRetrieved);
		    return this;
	    }

	    public IRemoteConfigure OnResourceDeleted(Func<HttpResponseMessage, Task> resourceDeleted)
	    {
		    ResourceDeletedCallbacks.Add(resourceDeleted);
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

            // build HttpClient factory
            SessionFactoryConfiguration.HttpClientFactory = () =>
            {
	            var innerHandler = HasDevDefinedCallbacks
		            ? new DefaultHttpMessageHandler(new HttpMessageCallbackHandler(
			            HttpResponseMessageCallbacks.ToArray(),
			            ResourceCreatedCallbacks.ToArray(),
			            ResourceUpdatedCallbacks.ToArray(),
			            ResourceRetrievedCallbacks.ToArray(),
			            ResourceDeletedCallbacks.ToArray(),
			            CreateHttpMessageHandler?.Invoke()))
		            : new DefaultHttpMessageHandler(CreateHttpMessageHandler?.Invoke());

				var client = (ClientCreator ?? (() => new HttpClient(innerHandler)))();

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