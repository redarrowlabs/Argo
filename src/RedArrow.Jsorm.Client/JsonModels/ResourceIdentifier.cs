using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Client.JsonModels
{
    public class ResourceIdentifier : JModel, IMetaDecorated
    {
        internal ResourceIdentifier() { }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Meta { get; set; }
    }
}