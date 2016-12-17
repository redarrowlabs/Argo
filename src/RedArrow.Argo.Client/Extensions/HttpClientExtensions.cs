using System.Net.Http;
using System.Threading.Tasks;

namespace RedArrow.Argo.Client.Extensions
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
        {
            return client.SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
            {
                Content = content
            });
        }
    }
}