using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RedArrow.Argo.Client.Tests
{
    public class MockRequestHandler : HttpMessageHandler
    {
	    private readonly IDictionary<Uri, Func<HttpRequestMessage, Task<HttpResponseMessage>>> _mockRequests;

        public bool Disposed { get; private set; }

        public int RequestsSent { get; set; }

	    public MockRequestHandler()
	    {
		    _mockRequests = new Dictionary<Uri, Func<HttpRequestMessage, Task<HttpResponseMessage>>>();
	    }

		public void Setup(Uri uri, Func<HttpRequestMessage, Task<HttpResponseMessage>> mock)
	    {
		    _mockRequests.Add(uri, mock);
	    }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestsSent++;

	        Func<HttpRequestMessage, Task<HttpResponseMessage>> mock;
	        if (_mockRequests.TryGetValue(request.RequestUri, out mock))
	        {
		        return mock(request);
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