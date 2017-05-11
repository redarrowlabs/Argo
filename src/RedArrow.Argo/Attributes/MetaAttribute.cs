using System;

namespace RedArrow.Argo.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MetaAttribute : Attribute
    {
        public MetaAttribute()
        {
        }

        public MetaAttribute(string metaName)
        {
        }
    }
}