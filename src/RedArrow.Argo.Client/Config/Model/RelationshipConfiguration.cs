using System.Linq;
using System.Reflection;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Extensions;

namespace RedArrow.Argo.Client.Config.Model
{
    public class RelationshipConfiguration
    {
        public string RelationshipName { get; }
        public PropertyInfo Property { get; }

        internal RelationshipConfiguration(PropertyInfo property)
        {
            Property = property;
            RelationshipName = Property.CustomAttributes
                .Single(a => a.AttributeType == typeof(HasManyAttribute) 
                    || a.AttributeType == typeof(HasOneAttribute))
                .ConstructorArguments
                .Select(arg => arg.Value as string)
                .FirstOrDefault() ?? Property.Name.Camelize();
        }
    }
}
