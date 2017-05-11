using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Extensions;
using System.Collections.Generic;

namespace RedArrow.Argo.Client.Model
{
    public class Resource : ResourceIdentifier
    {
        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Attributes { get; set; }

        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, Relationship> Relationships { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Links { get; set; }

        internal Resource()
        {
        }

        internal static Resource FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Resource>(json);
        }

        public void Patch(Resource patch)
        {
            GetAttributes().Merge(patch.Attributes);
            patch.Relationships?.Each(kvp => GetRelationships()[kvp.Key] = kvp.Value);
            patch.Meta?.Each(kvp => GetMeta()[kvp.Key] = kvp.Value);
        }

        public JObject GetAttributes()
        {
            return Attributes ?? (Attributes = new JObject());
        }

        public void SetAttribute(string attrName, object value)
        {
            GetAttributes()[attrName] = JToken.FromObject(value);
        }

        public IDictionary<string, Relationship> GetRelationships()
        {
            return Relationships ?? (Relationships = new Dictionary<string, Relationship>());
        }

        public IDictionary<string, JToken> GetLinks()
        {
            return Links ?? (Links = new Dictionary<string, JToken>());
        }

        public IDictionary<string, JToken> GetMeta()
        {
            return Meta ?? (Meta = new Dictionary<string, JToken>());
        }

        public void SetMeta(string metaName, object value)
        {
            GetMeta()[metaName] = JToken.FromObject(value);
        }

    }

    public static class ResourceExtensions
    {
        internal static ResourceIdentifier ToResourceIdentifier(this Resource resource)
        {
            return new ResourceIdentifier
            {
                Type = resource.Type,
                Id = resource.Id,
                Meta = resource.Meta
            };
        }
    }
}