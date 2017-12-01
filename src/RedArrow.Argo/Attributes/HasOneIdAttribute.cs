using System;

namespace RedArrow.Argo.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HasOneIdAttribute : Attribute
    {
        public HasOneIdAttribute()
        {
        }

        public HasOneIdAttribute(string rltnName)
        {
        }
    }
}
