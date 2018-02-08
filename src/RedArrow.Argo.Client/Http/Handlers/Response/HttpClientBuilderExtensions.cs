using System;
using RedArrow.Argo.Client.Config.Pipeline;

namespace RedArrow.Argo.Client.Http.Handlers.Response
{
    public static class HttpClientBuilderExtensions
    {
        [Obsolete("Instead try Use(new CustomResponseListener) on the Argo fluent config.")]
        public static IHttpClientBuilder UseResponseHandler(
            this IHttpClientBuilder builder,
            ResponseHandlerOptions options)
        {
            return builder.Use<ResponseHandler>(options);
        }
    }
}