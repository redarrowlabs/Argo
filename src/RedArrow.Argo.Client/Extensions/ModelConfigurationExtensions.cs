using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using RedArrow.Argo.Client.Config.Model;

namespace RedArrow.Argo.Client.Extensions
{
    internal static class ModelConfigurationExtensions
    {
        public static HttpRequestMessage CreateGetRequest(this ModelConfiguration config, Guid id)
        {
            var resourceType = config.ResourceType;
            var path = $"{resourceType}/{id}";

            var queryParams = new List<string>();

            // includes (eager-load)
            var eagerRltns = config.HasOneProperties.Values
                    .Where(has1 => has1.Eager)
                    .Select(has1 => has1.AttributeName);
            //.Concat(config.HasManyProperties.Values...
            if (eagerRltns.Any())
            {
                queryParams.Add($"include={string.Join(",", eagerRltns)}");
            }

            //TODO: add other query params for other api features

            if (queryParams.Any())
            {
                path += $"?{string.Join("&", queryParams)}";
            }

            return new HttpRequestMessage(HttpMethod.Get, path);
        }
    }
}