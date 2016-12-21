namespace RedArrow.Argo.Client.JsonModels
{
    public class ResourceRootSingle : BaseResourceRoot<Resource>
    {
        internal ResourceRootSingle() { }

        internal static ResourceRootSingle FromResource(Resource resource)
        {
            return new ResourceRootSingle
            {
                Data = resource
            };
        }
    }
}