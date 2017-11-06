using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace RedArrow.Argo.Client.Model
{
    public class Link : JModel, IMetaDecorated
    {
        [JsonProperty("href", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Href { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Meta { get; set; }

        internal Link()
        {
        }
    }
}