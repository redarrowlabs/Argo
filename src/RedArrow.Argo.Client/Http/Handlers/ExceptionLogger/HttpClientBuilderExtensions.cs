using RedArrow.Argo.Client.Config.Pipeline;

namespace RedArrow.Argo.Client.Http.Handlers.ExceptionLogger
{
	public static class HttpClientBuilderExtensions
	{
		public static IHttpClientBuilder UseExceptionLogger(this IHttpClientBuilder builder)
		{
			return builder.Use<ExceptionLoggerHandler>();
		}
	}
}
