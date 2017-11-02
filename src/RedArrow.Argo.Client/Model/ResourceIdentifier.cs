using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Model;

namespace RedArrow.Argo.Client.Model
{
    public class ResourceIdentifier : JModel, IMetaDecorated, IResourceIdentifier
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Meta { get; set; }

        internal ResourceIdentifier()
        {
        }
    }
}