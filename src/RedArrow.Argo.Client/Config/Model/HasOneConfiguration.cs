using System;
using System.Reflection;
using RedArrow.Argo.Attributes;

namespace RedArrow.Argo.Client.Config.Model
{
	public class HasOneConfiguration : RelationshipConfiguration
	{
		public HasOneConfiguration(PropertyInfo property) : base(property)
		{
		}

		protected override Type GetAttributeType()
		{
			return typeof(HasOneAttribute);
		}
	}
}
