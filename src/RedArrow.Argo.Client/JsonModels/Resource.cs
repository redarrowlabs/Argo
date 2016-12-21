﻿using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedArrow.Argo.Client.JsonModels
{
    public class Resource : ResourceIdentifier
    {
        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Attributes { get; set; }

        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, Relationship> Relationships { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Links { get; set; }

        internal Resource() { }

        internal static Resource FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Resource>(json);
        }

        public JObject GetAttributes()
        {
            return Attributes ?? (Attributes = new JObject());
        }

        public IDictionary<string, Relationship> GetRelationships()
        {
            return Relationships ?? (Relationships = new Dictionary<string, Relationship>());
        }

        public IDictionary<string, JToken> GetLinks()
        {
            return Links ?? (Links = new Dictionary<string, JToken>());
        }
    }
}