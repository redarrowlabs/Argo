using RedArrow.Jsorm.Attributes;
using RedArrow.Jsorm.Extensions;
using System.Linq;
using System.Reflection;

namespace RedArrow.Jsorm.Config.Model
{
    internal class HasManyConfiguration
    {
        public string AttributeName { get; }
        public PropertyInfo PropertyInfo { get; }
        public bool Eager { get; }

        public HasManyConfiguration(PropertyInfo propInfo)
        {
            PropertyInfo = propInfo;

            var attr = PropertyInfo
                .CustomAttributes
                .Single(x => x.AttributeType == typeof(HasManyAttribute));

            AttributeName = attr
                .ConstructorArguments
                .Where(x => x.ArgumentType == typeof(string))
                .Select(x => (string)x.Value)
                .FirstOrDefault() ?? propInfo.Name.Camelize();

            Eager = attr
                .ConstructorArguments
                .Where(x => x.ArgumentType == typeof(LoadStrategy))
                .Select(x => (LoadStrategy)x.Value)
                .FirstOrDefault() == LoadStrategy.Eager;
        }
    }
}