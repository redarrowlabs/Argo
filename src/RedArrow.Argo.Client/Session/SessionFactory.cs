using System;
using System.Collections.Generic;
using System.Net.Http;
using RedArrow.Argo.Client.Cache;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Http;
using RedArrow.Argo.Client.Session.Registry;

namespace RedArrow.Argo.Client.Session
{
    public class SessionFactory : ISessionFactory
    {
        private Func<HttpClient> HttpClientFactory { get; }
        private IEnumerable<ModelConfiguration> ModelConfigurations { get; }

        internal SessionFactory(Func<HttpClient> httpClientFactory, IEnumerable<ModelConfiguration> modelConfigurations)
        {
            HttpClientFactory = httpClientFactory;
            ModelConfigurations = modelConfigurations;
        }

        public ISession CreateSession(Action<HttpClient> configureClient = null)
        {
            // TODO? perhaps a way to use a real DI container here...
	        return new Session(
		        () =>
		        {
			        var client = HttpClientFactory();
			        configureClient?.Invoke(client);
			        return client;
		        },
		        new HttpRequestBuilder(),
		        new BasicCacheProvider(),
		        new ModelRegistry(ModelConfigurations));
        }
    }
}