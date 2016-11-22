using System;

namespace RedArrow.Jsorm.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HasOneAttribute : Attribute
    {
        public HasOneAttribute()
        {
        }

        public HasOneAttribute(string attrName)
        {
        }

		// lazy/eager config will be "made publich" later...
        internal HasOneAttribute(LoadStrategy strategy)
        {
        }

        internal HasOneAttribute(string attrName, LoadStrategy strategy)
        {
        }
    }
}