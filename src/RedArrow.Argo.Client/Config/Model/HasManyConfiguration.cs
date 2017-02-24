using System;
using System.Reflection;
using RedArrow.Argo.Attributes;

namespace RedArrow.Argo.Client.Config.Model
{
    public class HasManyConfiguration : RelationshipConfiguration
    {
	    public HasManyConfiguration(PropertyInfo propInfo) : base(propInfo)
	    {
	    }

		protected override Type GetAttributeType()
		{
			return typeof(HasManyAttribute);
		}
    }
}
