using System;

namespace RedArrow.Jsorm.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HasOneAttribute : Attribute
    {
        public HasOneAttribute()
        {
        }

        public HasOneAttribute(string type)
        {
        }

        public HasOneAttribute(LoadStrategy strategy)
        {
        }

        public HasOneAttribute(string type, LoadStrategy strategy)
        {
        }
    }
}