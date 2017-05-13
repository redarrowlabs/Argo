using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Extensions;
using System.Linq;
using System.Reflection;

namespace RedArrow.Argo.Client.Config.Model
{
    public class MetaConfiguration
    {
        public string MetaName { get; }
        public PropertyInfo Property { get; }

        internal MetaConfiguration(PropertyInfo property)
        {
            Property = property;
            MetaName = Property.CustomAttributes
                           .Single(a => a.AttributeType == typeof(MetaAttribute))
                           .ConstructorArguments
                           .Select(arg => arg.Value as string)
                           .FirstOrDefault() ?? Property.Name.Camelize();
        }
    }
}