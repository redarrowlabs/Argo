using RedArrow.Jsorm.Client.Cache;
using RedArrow.Jsorm.Client.Config.Model;
using RedArrow.Jsorm.Client.Session.Registry;
using System;
using System.Collections.Generic;
using System.Net.Http;
using RedArrow.Jsorm.Client.Http;

namespace RedArrow.Jsorm.Client.Session
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