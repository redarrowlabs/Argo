using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace RedArrow.Argo.Client.Model
{
    public class Error : JModel, IMetaDecorated
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public ErrorLink Links { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("detail", NullValueHandling = NullValueHandling.Ignore)]
        public string Detail { get; set; }

        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        public ErrorSource Source { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Meta { get; set; }

        internal Error()
        {
        }
    }
}