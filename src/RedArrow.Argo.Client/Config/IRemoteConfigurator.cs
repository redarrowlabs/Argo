using System;
using System.Net.Http;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Config.Pipeline;
using RedArrow.Argo.Client.Http.Handlers.Request;
using RedArrow.Argo.Client.Http.Handlers.Response;

namespace RedArrow.Argo.Client.Config
{
    public interface IRemoteConfigurator : IFluentConfigurator
    {
        IRemoteConfigurator Configure(Action<HttpClient> httpClient);

        IRemoteConfigurator ConfigureAsync(Func<HttpClient, Task> httpClient);

        IRemoteConfigurator Configure(Action<IHttpClientBuilder> builder);

        IRemoteConfigurator Use(HttpRequestModifier httpRequestModifier);

        /// <summary>
        /// Argo does NOT wait for listener tasks to complete
        /// </summary>
        /// <param name="httpResponseListener"></param>
        /// <returns></returns>
        IRemoteConfigurator Use(HttpResponseListener httpResponseListener);
    }
}