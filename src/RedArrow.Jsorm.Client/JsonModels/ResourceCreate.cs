using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Client.JsonModels
{
    internal class ResourceCreate : JModel, IMetaDecorated
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Meta { get; set; }

        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Attributes { get; set; }

        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, Relationship> Relationships { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Links { get; set; }

        public static Resource FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Resource>(json);
        }
    }
}