using RedArrow.Jsorm.Attributes;
using RedArrow.Jsorm.Extensions;
using System.Linq;
using System.Reflection;

namespace RedArrow.Jsorm.Config
{
    internal class PropertyConfiguration
    {
        public string AttributeName { get; }
        public PropertyInfo PropertyInfo { get; }

        public PropertyConfiguration(PropertyInfo propInfo)
        {
            PropertyInfo = propInfo;
            AttributeName = (PropertyInfo
                .CustomAttributes
                .Single(x => x.AttributeType == typeof(PropertyAttribute))
                .ConstructorArguments.Select(x => x.Value as string)
                .FirstOrDefault() ?? propInfo.Name)
                .Camelize();
        }
    }
}