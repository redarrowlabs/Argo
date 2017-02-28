using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace RedArrow.Argo.Extensions
{
    public static class PropertyDefinitionExtensions
    {
        public static FieldReference BackingField(this PropertyDefinition propDef)
        {
            return propDef
                ?.GetMethod
                ?.Body
                ?.Instructions
                ?.SingleOrDefault(x => x.OpCode == OpCodes.Ldfld)
                ?.Operand as FieldReference;
        }

        public static string JsonApiName(this PropertyDefinition propertyDef, TypeSystem typeSystem, string customAttrFullName)
        {
            var propAttr = propertyDef.CustomAttributes.GetAttribute(customAttrFullName);
            return propAttr.ConstructorArguments
                .Where(x => x.Type == typeSystem.String)
                .Select(x => x.Value as string)
                .SingleOrDefault() ?? propertyDef.Name.Camelize();
        }
    }
}
