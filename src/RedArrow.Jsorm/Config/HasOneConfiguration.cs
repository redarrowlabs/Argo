using System.Reflection;

namespace RedArrow.Jsorm.Config
{
    internal class HasOneConfiguration : PropertyConfiguration
    {
        public HasOneConfiguration(PropertyInfo propInfo) : base(propInfo)
        {
        }
    }
}