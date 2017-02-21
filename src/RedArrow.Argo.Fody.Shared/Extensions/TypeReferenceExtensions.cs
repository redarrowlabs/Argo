using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace RedArrow.Argo.Extensions
{
    public static class TypeReferenceExtensions
    {
        private static readonly IList<string> CeqStructNames;

        static TypeReferenceExtensions()
        {
            CeqStructNames = new List<string>
            {
                typeof (int).Name,
                typeof (uint).Name,
                typeof (long).Name,
                typeof (ulong).Name,
                typeof (float).Name,
                typeof (double).Name,
                typeof (bool).Name,
                typeof (short).Name,
                typeof (ushort).Name,
                typeof (byte).Name,
                typeof (sbyte).Name,
                typeof (char).Name,
            };
        }

        public static MethodReference InequalityOperator(this TypeReference self)
        {
            return self.Resolve()
                ?.Methods
                .Where(x => x.IsStatic)
                .Where(x => x.IsSpecialName)
                .Where(x => x.IsPublic)
                .SingleOrDefault(x => x.Name == "op_Inequality");
        }

        public static bool SupportsCeq(this TypeReference typeReference)
        {
            if (CeqStructNames.Contains(typeReference.Name))
            {
                return true;
            }
            if (typeReference.IsArray)
            {
                return false;
            }
            if (typeReference.ContainsGenericParameter)
            {
                return false;
            }
            var typeDefinition = typeReference.Resolve();
            if (typeDefinition == null)
            {
                throw new Exception($"Could not resolve '{typeReference.FullName}'.");
            }
            if (typeDefinition.IsEnum)
            {
                return true;
            }
            return !typeDefinition.IsValueType;
        }
    }
}