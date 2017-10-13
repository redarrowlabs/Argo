using System;

namespace RedArrow.Argo.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HasManyIdsAttribute : Attribute
    {
        public HasManyIdsAttribute()
        {
        }

        public HasManyIdsAttribute(string rltnName)
        {
        }
    }
}