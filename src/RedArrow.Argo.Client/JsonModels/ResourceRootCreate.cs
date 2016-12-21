using Newtonsoft.Json.Linq;

namespace RedArrow.Argo.Client.JsonModels
{
    public class ResourceRootCreate : BaseResourceRoot<ResourceCreate>
    {
        internal ResourceRootCreate() { }

        internal static ResourceRootCreate FromAttributes(string type, JObject attributes)
        {
            return new ResourceRootCreate
            {
                Data = new ResourceCreate
                {
                    Type = type,
                    Attributes = attributes
                }
            };
        }
    }
}