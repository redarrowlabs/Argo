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

        public HasManyAttribute(LoadStrategy strategy)
        {
        }

        public HasManyAttribute(string type, LoadStrategy strategy)
        {
        }
    }
}