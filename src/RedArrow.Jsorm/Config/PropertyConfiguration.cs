using RedArrow.Jsorm.Attributes;
using RedArrow.Jsorm.Extensions;
using System.Linq;
using System.Reflection;

namespace RedArrow.Jsorm.Config
{
    public class PropertyConfiguration
    {
        public string AttributeName { get; }
        public PropertyInfo PropertyInfo { get; }

        internal PropertyConfiguration(PropertyInfo propInfo)
        {
            PropertyInfo = propInfo;
            AttributeName = PropertyInfo
                .CustomAttributes
                .Single(x => x.AttributeType == typeof(PropertyAttribute))
                .ConstructorArguments.Select(x => (string)x.Value)
                .FirstOrDefault() ?? propInfo.Name.Camelize();
        }
    }
}