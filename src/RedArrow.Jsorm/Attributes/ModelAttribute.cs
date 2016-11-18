using System;

namespace RedArrow.Jsorm.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ModelAttribute : Attribute
    {
        public ModelAttribute()
        {
        }

        public ModelAttribute(string type)
        {
        }
    }
}