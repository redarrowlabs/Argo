﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedArrow.Argo.Client.JsonModels
{
    internal class Relationship : JModel, IMetaDecorated
    {
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Links { get; set; }

        [JsonProperty("data")]
        public JToken Data { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Meta { get; set; }

        public static Relationship FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Relationship>(json);
        }
    }
}