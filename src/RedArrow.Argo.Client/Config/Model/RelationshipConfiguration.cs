using System;
using System.Linq;
using System.Reflection;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Extensions;

namespace RedArrow.Argo.Client.Config.Model
{
    public abstract class RelationshipConfiguration
    {
        public string RelationshipName { get; }
        public PropertyInfo PropertyInfo { get; }
        public Type RelationshipType { get; }
        public bool Eager { get; }

        internal RelationshipConfiguration(PropertyInfo propInfo)
        {
            PropertyInfo = propInfo;

            var attr = PropertyInfo
                .CustomAttributes
                .Single(x => x.AttributeType == GetAttributeType());

            RelationshipName = attr
                                   .ConstructorArguments
                                   .Where(x => x.ArgumentType == typeof(string))
                                   .Select(x => (string) x.Value)
                                   .FirstOrDefault() ?? propInfo.Name.Camelize();

            RelationshipType = PropertyInfo.PropertyType.GenericTypeArguments.Length > 0
                ? PropertyInfo.PropertyType.GenericTypeArguments[0]
                : PropertyInfo.PropertyType;

            Eager = attr
                        .ConstructorArguments
                        .Where(x => x.ArgumentType == typeof(LoadStrategy))
                        .Select(x => (LoadStrategy) x.Value)
                        .FirstOrDefault() == LoadStrategy.Eager;
        }

        protected abstract Type GetAttributeType();
    }
}