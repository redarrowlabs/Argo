using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
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
        private JsonSerializerSettings JsonSettings { get; }

        internal SessionFactory(
            Func<HttpClient> httpClientFactory,
            IEnumerable<ModelConfiguration> modelConfigurations,
            JsonSerializerSettings jsonSettings)
        {
            HttpClientFactory = httpClientFactory;
            ModelConfigurations = modelConfigurations;
            JsonSettings = jsonSettings;
        }

        public ISession CreateSession(Action<HttpClient> configureClient = null)
        {
            var modelRegistry = new ModelRegistry(ModelConfigurations, JsonSettings);
            return new Session(() =>
                {
                    var client = HttpClientFactory();
                    configureClient?.Invoke(client);
                    return client;
                },
                new HttpRequestBuilder(JsonSettings),
                new BasicCacheProvider(modelRegistry),
                modelRegistry,
                JsonSettings);
        }
    }
}