namespace RedArrow.Argo.Client.JsonModels
{
    internal class ResourceRootSingle : BaseResourceRoot<Resource>
    {
        public static ResourceRootSingle FromResource(Resource resource)
        {
            return new ResourceRootSingle
            {
                Data = resource
            };
        }
    }
}