using System.Collections.Generic;
using RedArrow.Argo.Client.Extensions;

namespace RedArrow.Argo.Client.Model
{
    public class ResourceRootSingle : BaseResourceRoot<Resource>
    {
        internal ResourceRootSingle()
        {
        }

        internal static ResourceRootSingle FromResource(Resource resource, IEnumerable<Resource> included = null)
        {
            var root = new ResourceRootSingle {Data = resource};
            if (!included.IsNullOrEmpty())
            {
                root.Included = included;
            }
            return root;
        }
    }
}