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
    }
}
