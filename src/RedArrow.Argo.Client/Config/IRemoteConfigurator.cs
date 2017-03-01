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

        IRemoteConfigure UseMessageHandler(Func<HttpMessageHandler> createHandler);

	    IRemoteConfigure OnHttpResponse(Func<HttpResponseMessage, Task> responseReceived);
		IRemoteConfigure OnResourceCreated(Func<HttpResponseMessage, Task> resourceCreated);
		IRemoteConfigure OnResourceUpdated(Func<HttpResponseMessage, Task> resourceUpdated);
		IRemoteConfigure OnResourceRetreived(Func<HttpResponseMessage, Task> resourceRetrieved);
		IRemoteConfigure OnResourceDeleted(Func<HttpResponseMessage, Task> resourceDeleted);
	}
}