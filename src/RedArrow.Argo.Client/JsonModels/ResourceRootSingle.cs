using System.Collections.Generic;

namespace RedArrow.Argo.Client.JsonModels
{
    public class ResourceRootSingle : BaseResourceRoot<Resource>
    {
        internal ResourceRootSingle() { }

        internal static ResourceRootSingle FromResource(Resource resource, IEnumerable<Resource> included)
        {
            return new ResourceRootSingle
            {
                Data = resource,
                Included = included
            };
        }
    }
}