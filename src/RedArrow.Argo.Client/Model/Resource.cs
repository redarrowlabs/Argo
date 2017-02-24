using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedArrow.Argo.Client.Model
{
    public class Resource : ResourceIdentifier
	{
		private static readonly string ArgoVersion = typeof(Resource).GetTypeInfo().Assembly.GetName().Version.ToString();

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

	    internal static Resource FromType(string type)
	    {
		    return new Resource
		    {
			    Id = Guid.NewGuid(),
			    Type = type,
			    Meta = new Dictionary<string, JToken>
			    {
				    {"$ccv", JToken.FromObject(ArgoVersion)} //client created version
			    }
		    };
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