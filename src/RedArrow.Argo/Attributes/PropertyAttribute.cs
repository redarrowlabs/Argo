using System;

namespace RedArrow.Argo.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyAttribute : Attribute
    {
        public PropertyAttribute()
        {
        }

        public PropertyAttribute(string attrName)
        {
        }
    }
}