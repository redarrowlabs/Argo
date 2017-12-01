using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace RedArrow.Argo.Extensions
{
    public static class CecilExtensions
    {
        public static IEnumerable<CustomAttribute> GetAttributes(
            this IEnumerable<CustomAttribute> attributes,
            string attributeName)
        {
            return attributes.Where(attribute =>
                attribute
                .Constructor
                .DeclaringType
                .FullName == attributeName);
        }

        public static CustomAttribute GetAttribute(
            this IEnumerable<CustomAttribute> attributes,
            string attributeName)
        {
            return attributes.FirstOrDefault(attribute =>
                attribute
                .Constructor
                .DeclaringType
                .FullName == attributeName);
        }

        public static bool ContainsAttribute(this IEnumerable<CustomAttribute> attributes, string attributeName)
        {
            return attributes.Any(attribute => attribute.Constructor.DeclaringType.FullName == attributeName);
        }
    }
}