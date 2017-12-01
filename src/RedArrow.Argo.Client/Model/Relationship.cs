using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RedArrow.Argo.Client.Model
{
    public class Relationship : JModel, IMetaDecorated
    {
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Links { get; set; }

        [JsonProperty("data")]
        public JToken Data { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Meta { get; set; }

        internal Relationship()
        {
        }
    }
}