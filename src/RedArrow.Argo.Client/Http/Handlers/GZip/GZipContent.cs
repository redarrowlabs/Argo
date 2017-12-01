using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace RedArrow.Argo.Client.Http.Handlers.GZip
{
    internal class GZipContent : ByteArrayContent
    {
        public GZipContent(Stream contentStream) :
            base(CompressStream(contentStream))
        {
            Headers.ContentEncoding.Add("gzip");
        }

        private static byte[] CompressStream(Stream s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            using (var ms = new MemoryStream())
            {
                using (var gs = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    s.CopyTo(gs);
                }
                return ms.ToArray();
            }
        }
    }
}