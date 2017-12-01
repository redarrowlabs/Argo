using System;

namespace RedArrow.Argo.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HasOneAttribute : Attribute
    {
        public HasOneAttribute()
        {
        }

        public HasOneAttribute(string rltnName)
        {
        }

        public HasOneAttribute(LoadStrategy strategy)
        {
        }

        public HasOneAttribute(string rltnName, LoadStrategy strategy)
        {
        }
    }
}