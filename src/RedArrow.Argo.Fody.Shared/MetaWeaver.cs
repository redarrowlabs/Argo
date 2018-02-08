using RedArrow.Argo.Extensions;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void WeaveMeta(ModelWeavingContext context)
        {
            foreach (var propertyDef in context.MappedMeta)
            {
                if (propertyDef.CustomAttributes.ContainsAttribute(Constants.Attributes.Property))
                {
                    LogError($"Property {propertyDef.FullName} cannot be included in both attributes and meta");
                }
            }
        }
    }
}