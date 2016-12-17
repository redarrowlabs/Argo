using System;

namespace RedArrow.Argo.Attributes
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