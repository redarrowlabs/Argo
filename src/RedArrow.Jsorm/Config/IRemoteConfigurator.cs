using System;
using System.Net.Http;

namespace RedArrow.Jsorm.Config
{
    public interface IRemoteCreator : IRemoteConfigure
    {
        IRemoteConfigure Create(Func<HttpClient> createClient);
    }

    public interface IRemoteConfigure : IFluentConfigurator
    {
        IRemoteConfigure Configure(Action<HttpClient> configureClient);
    }
}