using System;
using System.Net.Http;

namespace RedArrow.Jsorm.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static Guid GetResourceId(this HttpResponseMessage response)
        {
            var locationHeader = response.Headers.Location.ToString();
            var idStr = locationHeader.Substring(locationHeader.Length - 36, 36);
            return Guid.Parse(idStr);
        }
    }
}