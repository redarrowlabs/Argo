using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void VerifyIdProperty(ModelWeavingContext context)
        {
            if (context.IdPropDef != null && context.IdPropDef.GetMethod?.ReturnType.Resolve() != _guidTypeDef)
            {
                LogError($"{context.IdPropDef.FullName} [Id] property must have a System.Guid getter");
            }

            // if id property doesn't have a setter, try to add one
            if (context.IdPropDef != null && context.IdPropDef.SetMethod == null)
            {
                LogInfo($"{context.IdPropDef.FullName} has no setter.  Attempting to resolve...");

                var getterBackingField = context
                    .IdPropDef
                    ?.GetMethod
                    ?.Body
                    ?.Instructions
                    ?.SingleOrDefault(x => x.OpCode == OpCodes.Ldfld)
                    ?.Operand as FieldReference;

                if (getterBackingField != null)
                {
                    var setter = new MethodDefinition(
                        $"set_{context.IdPropDef.Name}",
                        MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                        context.ImportReference(TypeSystem.Void));

                    setter.Parameters.Add(
                        new ParameterDefinition(
                            "value",
                            ParameterAttributes.None,
                            context.IdPropDef.PropertyType));
                    setter.SemanticsAttributes = MethodSemanticsAttributes.Setter;

                    var proc = setter.Body.GetILProcessor();
                    proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
                    proc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
                    proc.Emit(OpCodes.Stfld, getterBackingField); // this.backingField = value;
                    proc.Emit(OpCodes.Ret); // return

                    context.Methods.Add(setter);

                    context.IdPropDef.SetMethod = setter;
                }
                else
                {
                    throw new Exception($"Model {context.ModelTypeRef.FullName} [Id] property '{context.IdPropDef?.Name}' has no setter. This property must have a private or protected setter");
                }

                LogInfo($"Successfully added private setter to {context.IdPropDef}");
            }
        }
    }
}