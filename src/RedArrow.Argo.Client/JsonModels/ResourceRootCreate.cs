using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace RedArrow.Argo.Client.JsonModels
{
    public class ResourceRootCreate : BaseResourceRoot<ResourceCreate>
    {
        internal ResourceRootCreate() { }

        internal static ResourceRootCreate FromObject(string type, JObject attributes, IEnumerable<Resource> included, IDictionary<string, Relationship> relationships)
        {
            return new ResourceRootCreate
            {
                Data = new ResourceCreate
                {
                    Type = type,
                    Attributes = attributes,
                    Relationships = relationships ?? new Dictionary<string, Relationship>()
                },
                Included = included ?? new List<Resource>()
            };
        }
    }
}