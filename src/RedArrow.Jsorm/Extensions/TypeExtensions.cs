using System;
using System.Linq;
using System.Reflection;
using RedArrow.Jsorm.Infrastructure;

namespace RedArrow.Jsorm.Extensions
{
    public static class TypeExtensions
    {
        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
            if (type == null || type.GetTypeInfo().IsAbstract)
            {
                return null;
            }

            var result = type.GetTypeInfo()
                .DeclaredConstructors
                .FirstOrDefault(ctor => ctor.IsPublic && !ctor.GetParameters().Any());

            if (result == null)
            {
                throw new JsormException("A default (no-arg) constructor could not be found for: ", type);
            }

            return result;
        }
    }
}