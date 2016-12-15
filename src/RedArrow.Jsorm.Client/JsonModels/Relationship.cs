using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Client.JsonModels
{
    public class Relationship : JModel, IMetaDecorated
    {
        internal Relationship() { }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Links { get; set; }

        [JsonProperty("data")]
        public JToken Data { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Meta { get; set; }

        internal static Relationship FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Relationship>(json);
        }
    }
}