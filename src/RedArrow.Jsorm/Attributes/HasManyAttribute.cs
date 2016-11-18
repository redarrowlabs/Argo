using System;

namespace RedArrow.Jsorm.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class HasManyAttribute : Attribute
	{
		public HasManyAttribute()
		{
		}

		public HasManyAttribute(string type)
		{
		}
	}
}
