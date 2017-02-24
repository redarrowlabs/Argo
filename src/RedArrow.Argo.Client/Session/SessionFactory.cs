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

        public ISession CreateSession()
        {
            // TODO? perhaps a way to use a real DI container here...
            var modelRegistry = new ModelRegistry(ModelConfigurations);
            var httpRequestBuider = new HttpRequestBuilder(modelRegistry);

            return new Session(
                HttpClientFactory,
                httpRequestBuider,
                new BasicCacheProvider(),
                modelRegistry);
        }
    }
}