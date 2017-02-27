using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Extensions;

namespace RedArrow.Argo.Client.Http
{
	public class HttpMessageCallbackHandler : DelegatingHandler
	{
		private IEnumerable<Func<HttpResponseMessage, Task>> ResponseReceived { get; }
		private IEnumerable<Func<HttpResponseMessage, Task>> ResourceCreated { get; }
		private IEnumerable<Func<HttpResponseMessage, Task>> ResourceUpdated { get; }
		private IEnumerable<Func<HttpResponseMessage, Task>> ResourceRetrieved { get; }
		private IEnumerable<Func<HttpResponseMessage, Task>> ResourceDeleted { get; }

		private IDictionary<string, IEnumerable<Func<HttpResponseMessage, Task>>> Callbacks { get; }

		public HttpMessageCallbackHandler(
			IEnumerable<Func<HttpResponseMessage, Task>> responseReceived,
			IEnumerable<Func<HttpResponseMessage, Task>> resourceCreated,
			IEnumerable<Func<HttpResponseMessage, Task>> resourceUpdated,
			IEnumerable<Func<HttpResponseMessage, Task>> resourceRetrieved,
			IEnumerable<Func<HttpResponseMessage, Task>> resourceDeleted,
			HttpMessageHandler innerHandler) :
			base(innerHandler)
		{
			InnerHandler = innerHandler ?? new HttpClientHandler();

			Callbacks = new Dictionary<string, IEnumerable<Func<HttpResponseMessage, Task>>>
			{
				{null, responseReceived},
				{HttpMethod.Post.Method, resourceCreated},
				{"PATCH", resourceUpdated},
				{HttpMethod.Get.Method, resourceRetrieved},
				{HttpMethod.Delete.Method, resourceDeleted}
			};
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var response = await base.SendAsync(request, cancellationToken);

			var callbacks = new List<Func<HttpResponseMessage, Task>>(Callbacks[null]);
			var method = response.RequestMessage.Method.Method;
			if (Callbacks.ContainsKey(method))
			{
				callbacks.AddRange(Callbacks[method]);
			}

			if (!callbacks.Any()) return response;

			var copy = CopyResponse(response);
			// callbacks are run asyncronously
			callbacks.Each(x => Task.Run(() => x(copy), cancellationToken));
			return response;
		}

		private static HttpResponseMessage CopyResponse(HttpResponseMessage src)
		{
			var ret = new HttpResponseMessage(src.StatusCode)
			{
				Version = CopyVersion(src.Version),
				RequestMessage = CopyRequest(src.RequestMessage),
				ReasonPhrase = src.ReasonPhrase
			};
			//TODO: copy src.Content...
			src.Headers.Each(h => ret.Headers.Add(h.Key, h.Value));
			return ret;
		}

		private static HttpRequestMessage CopyRequest(HttpRequestMessage src)
		{
			var ret = new HttpRequestMessage(src.Method, src.RequestUri)
			{
				Version	= CopyVersion(src.Version)
			};
			//TODO: copy src.Content...
			src.Headers.Each(h => ret.Headers.Add(h.Key, h.Value));
			src.Properties.Each(kvp => ret.Properties.Add(kvp));
			return ret;
		}

		private static Version CopyVersion(Version src)
		{
			return new Version(src.Major, src.Minor, src.Build, src.Revision);
		}
	}
}
