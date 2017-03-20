using System;
using System.Net.Http;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Config.Pipeline;

namespace RedArrow.Argo.Client.Config
{
    public interface IRemoteConfigurator : IFluentConfigurator
    {
        IRemoteConfigurator Configure(Action<HttpClient> httpClient);

        IRemoteConfigurator ConfigureAsync(Func<HttpClient, Task> httpClient);
		
	    IRemoteConfigurator Configure(Action<IHttpClientBuilder> builder);
	}
}