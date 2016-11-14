using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedArrow.Jsorm.JsonModels
{
    public class ResourceRoot : JModel, IMetaDecorated
    {
        [JsonProperty("jsonapi", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> JsonApi { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public JToken Data { get; set; }

        [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Error> Errors { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Meta { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, JToken> Links { get; set; }

        [JsonProperty("included", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Resource> Included { get; set; }

        public static ResourceRoot FromErrors(params Error[] errors)
        {
            return new ResourceRoot
            {
                Errors = errors
            };
        }

        public static ResourceRoot FromResource(Resource resource)
        {
            return new ResourceRoot
            {
                Data = resource == null
                    ? JValue.CreateNull()
                    : JToken.FromObject(resource)
            };
        }

        public static ResourceRoot FromResources(IEnumerable<Resource> resources)
        {
            return new ResourceRoot
            {
                Data = resources == null
                    ? new JArray()
                    : JToken.FromObject(resources)
            };
        }

        public static ResourceRoot FromRelationship(Relationship relationship)
        {
            return new ResourceRoot
            {
                Links = relationship.Links,
                Data = relationship.Data,
                Meta = relationship.Meta
            };
        }

        public static ResourceRoot FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ResourceRoot>(json);
        }
    }
}