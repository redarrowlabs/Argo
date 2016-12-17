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

        // lazy/eager config will be "made publich" later...
        internal HasOneAttribute(LoadStrategy strategy)
        {
        }

        internal HasOneAttribute(string rltnName, LoadStrategy strategy)
        {
        }
    }
}