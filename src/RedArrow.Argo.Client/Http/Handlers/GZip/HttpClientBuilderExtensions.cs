using System.Net;
using System.Net.Http;
using RedArrow.Argo.Client.Config.Pipeline;

namespace RedArrow.Argo.Client.Http.Handlers.GZip
{
	public static class HttpClientBuilderExtensions
	{
		public static IHttpClientBuilder UseGZipCompression(this IHttpClientBuilder builder)
		{
			builder.ConfigureRequestHandler(x =>
			{
				var httpClientHandler = x as HttpClientHandler;
				if (httpClientHandler != null && httpClientHandler.SupportsAutomaticDecompression)
				{
					httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip;
				}
			});
			return builder;
		}
	}
}
