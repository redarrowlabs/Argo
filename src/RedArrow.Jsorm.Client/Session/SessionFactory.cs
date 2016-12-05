using RedArrow.Jsorm.Client.Cache;
using RedArrow.Jsorm.Client.Config.Model;
using RedArrow.Jsorm.Client.Session.Registry;
using System;
using System.Collections.Generic;
using System.Net.Http;

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
            return new Session(
                HttpClientFactory,
                new BasicCacheProvider(),
                new ModelRegistry(ModelConfigurations));
        }
    }
}