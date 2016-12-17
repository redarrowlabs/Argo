using System;

namespace RedArrow.Argo.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HasManyAttribute : Attribute
    {
        public HasManyAttribute()
        {
        }

        public HasManyAttribute(string rltnName)
        {
        }

        internal HasManyAttribute(LoadStrategy strategy)
        {
        }

        internal HasManyAttribute(string rltnName, LoadStrategy strategy)
        {
        }
    }
}