using RedArrow.Jsorm.Config.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace RedArrow.Jsorm.Session
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
            return new Session(HttpClientFactory, ModelConfigurations);
        }
    }
}