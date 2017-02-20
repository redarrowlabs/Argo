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

        public HasManyAttribute(LoadStrategy strategy)
        {
        }

        public HasManyAttribute(string rltnName, LoadStrategy strategy)
        {
        }
    }
}