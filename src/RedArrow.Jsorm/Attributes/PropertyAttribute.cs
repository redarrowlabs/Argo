using System;

namespace RedArrow.Jsorm.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class PropertyAttribute : Attribute
	{
		public PropertyAttribute()
		{
		}

		public PropertyAttribute(string attrName)
		{
		}
	}
}
