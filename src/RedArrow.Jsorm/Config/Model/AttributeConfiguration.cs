using RedArrow.Jsorm.Attributes;
using RedArrow.Jsorm.Extensions;
using System.Linq;
using System.Reflection;

namespace RedArrow.Jsorm.Config.Model
{
    public class AttributeConfiguration
    {
        public string AttributeName { get; }
        public PropertyInfo Property { get; }

        internal AttributeConfiguration(PropertyInfo property)
        {
            Property = property;
            AttributeName = Property.CustomAttributes
                .Single(a => a.AttributeType == typeof(PropertyAttribute))
                .ConstructorArguments
                .Select(arg => arg.Value as string)
                .FirstOrDefault() ?? Property.Name.Camelize();
        }
    }
}