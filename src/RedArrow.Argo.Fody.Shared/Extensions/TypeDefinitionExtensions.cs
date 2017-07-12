using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace RedArrow.Argo.Extensions
{
    public static class TypeDefinitionExtensions
    {
        public static List<PropertyDefinition> GetProperties(this TypeDefinition typeDef)
        {
            var properties = typeDef.Properties.ToList();

            if (typeDef.BaseType.Resolve() != typeDef.Module.ImportReference(typeof(object)).Resolve())
            {
                properties.AddRange(typeDef.BaseType.Resolve().GetProperties());
            }

            return properties;
        }
    }
}
