using System;
using System.Reflection;
using RedArrow.Argo.Attributes;

namespace RedArrow.Argo.Client.Config.Model
{
    public class HasOneConfiguration : RelationshipConfiguration
    {
        public FieldInfo IsInitializedFieldInfo { get; }

        public HasOneConfiguration(PropertyInfo property, FieldInfo isInitialized) : base(property)
        {
            IsInitializedFieldInfo = isInitialized;
        }

        protected override Type GetAttributeType()
        {
            return typeof(HasOneAttribute);
        }
    }
}