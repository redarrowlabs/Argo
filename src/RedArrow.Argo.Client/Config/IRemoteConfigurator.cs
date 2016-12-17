using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedArrow.Argo.Client.Config
{
    public interface IRemoteCreator : IRemoteConfigure
    {
        IRemoteConfigure Create(Func<HttpClient> createClient);
    }

    public interface IRemoteConfigure : IFluentConfigurator
    {
        IRemoteConfigure Configure(Action<HttpClient> httpClient);

        IRemoteConfigure ConfigureAsync(Func<HttpClient, Task> httpClient);

        IRemoteConfigure UseMessageHandler(HttpMessageHandler handler);
    }
}