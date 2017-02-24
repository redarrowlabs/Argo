using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedArrow.Argo.Client.Model
{
    public class ResourceCreate : JModel, IMetaDecorated
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

        internal ResourceCreate() { }

        internal static ResourceCreate FromResource(Resource resource)
        {
            return new ResourceCreate
            {
                Type = resource.Type,
                Attributes = resource.Attributes,
                Relationships = resource.Relationships,

                Links = resource.Links,
                Meta = resource.Meta
            };
        }
    }
}