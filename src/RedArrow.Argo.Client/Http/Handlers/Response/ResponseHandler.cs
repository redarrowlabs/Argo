using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Extensions;

namespace RedArrow.Argo.Client.Http.Handlers.Response
{
	public class ResponseHandler : DelegatingHandler
	{
		private ResponseHandlerOptions Options { get; }
		
		public ResponseHandler(ResponseHandlerOptions options)
		{
			Options = options;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var response = await base.SendAsync(request, cancellationToken);

			//intentionally not awaited
			Task.Run(() => ProcessResponse(response), cancellationToken);

			return response;
		}

		private async Task ProcessResponse(HttpResponseMessage response)
		{
			var processors = GetProcessors(response.RequestMessage.Method);
			if (processors.Any())
			{
				var copy = await CopyResponse(response);
				await Task.WhenAll(processors.Select(x => x(copy)).ToArray());
			}
		}

		private IEnumerable<Func<HttpResponseMessage, Task>> GetProcessors(HttpMethod method)
		{
			var ret = new List<Func<HttpResponseMessage, Task>>();

			if (Options.ResponseReceived != null)
			{
				ret.Add(Options.ResponseReceived);
			}

			switch (method.Method)
			{
				case "PATCH":
				{
					if (Options.ResourceUpdated != null)
					{
						ret.Add(Options.ResourceUpdated);
					}
					break;
				}
				case "POST":
				{
					if (Options.ResourceCreated != null)
					{
						ret.Add(Options.ResourceCreated);
					}
					break;
				}
				case "GET":
				{
					if (Options.ResourceRetrieved != null)
					{
						ret.Add(Options.ResourceRetrieved);
					}
					break;
				}
				case "DELETE":
				{
					if (Options.ResourceDeleted != null)
					{
						ret.Add(Options.ResourceDeleted);
					}
					break;
				}
			}

			return ret;
		} 

		private static async Task<HttpResponseMessage> CopyResponse(HttpResponseMessage src)
		{
			var ret = new HttpResponseMessage(src.StatusCode)
			{
				Version = CopyVersion(src.Version),
				RequestMessage = await CopyRequest(src.RequestMessage),
				ReasonPhrase = src.ReasonPhrase,
				Content = new StreamContent(await src.Content.ReadAsStreamAsync())
			};
			src.Headers.Each(h => ret.Headers.Add(h.Key, h.Value));
			return ret;
		}

		private static async Task<HttpRequestMessage> CopyRequest(HttpRequestMessage src)
		{
			var ret = new HttpRequestMessage(src.Method, src.RequestUri)
			{
				Version	= CopyVersion(src.Version),
				Content = new StreamContent(await src.Content.ReadAsStreamAsync())
			};
			src.Headers.Each(h => ret.Headers.Add(h.Key, h.Value));
			src.Properties.Each(kvp => ret.Properties.Add(kvp));
			return ret;
		}

		private static Version CopyVersion(Version src)
		{
			return new Version(src.ToString());
		}
	}
}
