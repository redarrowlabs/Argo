using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RedArrow.Argo.Client.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static Guid GetResourceId(this HttpResponseMessage response)
        {
            var locationHeader = response.Headers.Location.ToString();
            var idStr = locationHeader.Substring(locationHeader.Length - 36, 36);
            return Guid.Parse(idStr);
        }

		// deserializing from stream is more performant than loading a huge string into memory
		// http://www.newtonsoft.com/json/help/html/Performance.htm
		public static async Task<TModel> GetContentModel<TModel>(this HttpResponseMessage response)
	    {
			using (var s = await response.Content.ReadAsStreamAsync())
			using (var sr = new StreamReader(s))
			using (var jtr = new JsonTextReader(sr))
			{
				return new JsonSerializer().Deserialize<TModel>(jtr);
			}
		}
    }
}