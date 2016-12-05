using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Client.JsonModels
{
    internal class Link : JModel, IMetaDecorated
    {
        [JsonProperty("href", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Href { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Meta { get; set; }
    }
}