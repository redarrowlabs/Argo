using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RedArrow.Argo.Client.Tests
{
    public class FakeResponseHandler : DelegatingHandler
    {
        private readonly IDictionary<Uri, HttpResponseMessage> _fakeResponses = new Dictionary<Uri, HttpResponseMessage>();

        public bool Disposed { get; private set; }

        public void AddFakeResponse(Uri uri, HttpResponseMessage response)
        {
            _fakeResponses.Add(uri, response);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;
            if (_fakeResponses.TryGetValue(request.RequestUri, out response))
            {
                return Task.FromResult(response);
            }

            throw new Exception($"no response mapped for request {request.RequestUri}");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Disposed = true;
        }
    }
}