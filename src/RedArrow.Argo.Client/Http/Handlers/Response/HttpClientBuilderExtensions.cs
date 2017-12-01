using RedArrow.Argo.Client.Config.Pipeline;

namespace RedArrow.Argo.Client.Http.Handlers.Response
{
    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder UseResponseHandler(
            this IHttpClientBuilder builder,
            ResponseHandlerOptions options)
        {
            return builder.Use<ResponseHandler>(options);
        }
    }
}