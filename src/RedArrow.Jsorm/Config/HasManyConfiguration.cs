using System.Reflection;

namespace RedArrow.Jsorm.Config
{
    internal class HasManyConfiguration : PropertyConfiguration
    {
        public HasManyConfiguration(PropertyInfo propInfo) : base(propInfo)
        {
        }
    }
}