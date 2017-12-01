using System;
using System.Net.Http;

namespace RedArrow.Argo.Client.Session
{
    public interface ISessionFactory
    {
        ISession CreateSession(Action<HttpClient> configureClient = null);
    }
}