using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Http.Handlers.Request;
using RedArrow.Argo.Client.Session;

namespace RedArrow.Argo.Client.Config
{
    public class SessionFactoryConfiguration
    {
        internal Func<HttpClient> HttpClientFactory { get; set; }

        private ICollection<ModelConfiguration> ModelConfigurations { get; }

        private JsonSerializerSettings JsonSettings { get; set; }

        private HttpRequestModifier HttpRequestModifier { get; set; }

        internal SessionFactoryConfiguration()
        {
            ModelConfigurations = new List<ModelConfiguration>();
        }

        internal void Register(ModelConfiguration config)
        {
            ModelConfigurations.Add(config);
        }

        internal void Configure(JsonSerializerSettings jsonSettings)
        {
            JsonSettings = jsonSettings;
        }

        internal void Configure(HttpRequestModifier httpRequestModifier)
        {
            HttpRequestModifier = httpRequestModifier;
        }

        public ISessionFactory BuildSessionFactory()
        {
            return new SessionFactory(HttpClientFactory, ModelConfigurations, JsonSettings, HttpRequestModifier);
        }
    }
}