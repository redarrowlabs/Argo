using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedArrow.Argo.Client.Http.Handlers.Response
{
    public class ResponseHandlerOptions
    {
        public Func<HttpResponseMessage, Task> ResponseReceived { get; set; }
        public Func<HttpResponseMessage, Task> ResourceCreated { get; set; }
        public Func<HttpResponseMessage, Task> ResourceUpdated { get; set; }
        public Func<HttpResponseMessage, Task> ResourceRetrieved { get; set; }
        public Func<HttpResponseMessage, Task> ResourceDeleted { get; set; }
    }
}