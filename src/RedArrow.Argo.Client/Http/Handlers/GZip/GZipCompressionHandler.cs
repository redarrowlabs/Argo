using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RedArrow.Argo.Client.Http.Handlers.GZip
{
    public class GZipCompressionHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (var ms = new MemoryStream())
            using (var zipper = new GZipStream(ms, CompressionMode.Compress, true))
            {
                var requestBytes = await request.Content.ReadAsByteArrayAsync();
                await zipper.WriteAsync(requestBytes, 0, requestBytes.Length, cancellationToken);
                ms.Position = 0;
                var compressedBytes = new byte[ms.Length];
                await ms.ReadAsync(compressedBytes, 0, compressedBytes.Length, cancellationToken);

                var outStream = new MemoryStream(compressedBytes);
                var streamContent = new StreamContent(outStream);
                foreach (var header in request.Headers)
                {
                    streamContent.Headers.Add(header.Key, header.Value);
                }
                streamContent.Headers.ContentEncoding.Clear();
                streamContent.Headers.ContentEncoding.Add("gzip");
                streamContent.Headers.ContentLength = outStream.Length;

                request.Content = streamContent;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
