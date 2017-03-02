using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Logging;

namespace RedArrow.Argo.Client.Http
{
    public class DefaultHttpMessageHandler : DelegatingHandler
    {
        private static readonly ILog Log = LogProvider.For<DefaultHttpMessageHandler>();

        internal DefaultHttpMessageHandler(HttpMessageHandler innerHandler) :
            base(innerHandler ?? new HttpClientHandler())
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var hash = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            Log.Debug(() => $"request [{hash}]: {request}");
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                Log.Debug(() => $"response [{hash}]: {response}");
                return response;
            }
            catch (Exception ex)
            {
                Log.FatalException("request [{0}] failed unexpectedly", ex, hash);
                throw;
            }

        }
    }
}
