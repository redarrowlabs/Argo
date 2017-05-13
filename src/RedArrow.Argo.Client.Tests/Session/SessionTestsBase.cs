using System;
using System.Linq;
using System.Net.Http;
using Moq;
using Newtonsoft.Json;
using RedArrow.Argo.Client.Cache;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Http;
using RedArrow.Argo.Client.Session.Registry;

namespace RedArrow.Argo.Client.Tests.Session
{
    public abstract class SessionTestsBase
    {
        protected static Client.Session.Session CreateSubject(
            HttpMessageHandler mockHandler = null,
            IHttpRequestBuilder requestBuilder = null,
            ICacheProvider cacheProvider = null,
            IModelRegistry modelRegistry = null,
            JsonSerializerSettings jsonSettings = null)
        {
            if (mockHandler == null)
            {
                mockHandler = new HttpClientHandler();
            }

            return new Client.Session.Session(
                () => new HttpClient(mockHandler),
                requestBuilder ?? Mock.Of<IHttpRequestBuilder>(),
                cacheProvider ?? Mock.Of<ICacheProvider>(),
                modelRegistry ?? Mock.Of<IModelRegistry>(),
                jsonSettings ?? new JsonSerializerSettings());
        }

        protected static IModelRegistry CreateModelRegistry(params Type[] types)
        {
            return new ModelRegistry(types.Select(x => new ModelConfiguration(x)));
        }
    }
}