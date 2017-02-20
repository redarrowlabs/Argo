using System;
using System.Linq;
using System.Reflection;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Extensions;

namespace RedArrow.Argo.Client.Config.Model
{
    public class HasOneConfiguration
    {
        public string RelationshipName { get; }
        public PropertyInfo PropertyInfo { get; }
        public Type HasOneType { get; }
        public bool Eager { get; }

        internal HasOneConfiguration(PropertyInfo propInfo)
        {
            PropertyInfo = propInfo;

            var attr = PropertyInfo
                .CustomAttributes
                .Single(x => x.AttributeType == typeof(HasOneAttribute));

            RelationshipName = attr
                .ConstructorArguments
                .Where(arg => arg.ArgumentType == typeof(string))
                .Select(arg => arg.Value as string)
                .FirstOrDefault() ?? PropertyInfo.Name.Camelize();

            HasOneType = PropertyInfo.PropertyType.GenericTypeArguments.Length > 0 ? PropertyInfo.PropertyType.GenericTypeArguments[0] : PropertyInfo.PropertyType;

            Eager = attr
                .ConstructorArguments
                .Where(x => x.ArgumentType == typeof(LoadStrategy))
                .Select(x => (LoadStrategy)x.Value)
                .FirstOrDefault() == LoadStrategy.Eager;
        }
    }
}