using RedArrow.Jsorm.Config.Model;
using RedArrow.Jsorm.Session;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace RedArrow.Jsorm.Config
{
    public class SessionFactoryConfiguration
    {
        internal Func<HttpClient> HttpClientFactory { get; set; }

        private ICollection<ModelConfiguration> ModelConfigurations { get; }

        internal SessionFactoryConfiguration()
        {
            ModelConfigurations = new List<ModelConfiguration>();
        }

        internal void Register(ModelConfiguration config)
        {
            ModelConfigurations.Add(config);
        }

        public ISessionFactory BuildSessionFactory()
        {
            return new SessionFactory(HttpClientFactory, ModelConfigurations);
        }
    }
}