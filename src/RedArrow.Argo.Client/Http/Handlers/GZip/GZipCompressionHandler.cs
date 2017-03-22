using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RedArrow.Argo.Client.Http.Handlers.GZip
{
    public class GZipCompressionHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
	        if (request.Content != null)
	        {
		        var contentStream = await request.Content.ReadAsStreamAsync();
				var gzipContent = new GZipContent(contentStream);
		        foreach (var header in request.Content.Headers)
		        {
			        gzipContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
		        }
		        request.Content = gzipContent;
	        }

	        return await base.SendAsync(request, cancellationToken);
        }
    }
}
