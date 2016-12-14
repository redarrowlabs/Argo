using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RedArrow.Jsorm.Client.Logging;

namespace RedArrow.Jsorm.Client.Http
{
    public class DefaultHttpMessageHandler : DelegatingHandler
    {
        private static readonly ILog Log = LogProvider.For<DefaultHttpMessageHandler>();

        internal DefaultHttpMessageHandler(HttpMessageHandler innerHandler)
        {
            InnerHandler = innerHandler ?? new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var hash = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            Log.Debug(() => $"JSORM||request [{hash}]: {request}");
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                Log.Debug(() => $"JSORM||response [{hash}]: {response}");
                return response;
            }
            catch (Exception ex)
            {
                Log.FatalException("JSORM||request [{0}] failed unexpectedly", ex, hash);
                throw;
            }

        }
    }
}
