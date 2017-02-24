using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Client.Extensions;

namespace RedArrow.Argo.Client.Model
{
    public class ResourceRootCreate : BaseResourceRoot<ResourceCreate>
    {
        internal ResourceRootCreate() { }

        internal static ResourceRootCreate FromResource(Resource resource, IEnumerable<Resource> included)
        {
			var root = new ResourceRootCreate
			{
				Data = ResourceCreate.FromResource(resource)
			};

	        if (!included.IsNullOrEmpty())
	        {
		        root.Included = included;
	        }
	        return root;
        }

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