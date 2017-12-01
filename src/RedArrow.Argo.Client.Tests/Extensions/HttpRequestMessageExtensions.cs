using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RedArrow.Argo.Client.Tests.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static async Task<TModel> GetBody<TModel>(this HttpRequestMessage request)
        {
            var rawContent = await request.Content.ReadAsStringAsync();
            return JObject.Parse(rawContent).ToObject<TModel>();
        }
    }
}