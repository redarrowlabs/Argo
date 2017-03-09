using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void WeaveId(ModelWeavingContext context)
        {
            if (context.IdPropDef != null &&
                context.IdPropDef.GetMethod?.ReturnType.Resolve() != context.ImportReference(typeof(Guid)).Resolve())
            {
                LogError($"[Id] property must have a System.Guid getter: {context.IdPropDef.FullName} ");
            }

            // if id property doesn't have a setter, try to add one
            if (context.IdPropDef != null)
            {
                LogInfo($"Upserting [Id] property setter: {context.IdPropDef.FullName} ");

                var idBackingField = context
                    .IdPropDef
                    ?.GetMethod
                    ?.Body
                    ?.Instructions
                    ?.SingleOrDefault(x => x.OpCode == OpCodes.Ldfld)
                    ?.Operand as FieldReference;

                var setter = context.IdPropDef.SetMethod;
                if (setter == null)
                {
                    setter = new MethodDefinition(
                        $"set_{context.IdPropDef.Name}",
                        MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                        context.ImportReference(TypeSystem.Void));
                    setter.Parameters.Add(
                        new ParameterDefinition(
                            "value",
                            ParameterAttributes.None,
                            context.IdPropDef.PropertyType));
                    setter.SemanticsAttributes = MethodSemanticsAttributes.Setter;
                    context.Methods.Add(setter);
                    context.IdPropDef.SetMethod = setter;
                }
                else
                {
                    setter.Body.Instructions.Clear();
                }
                var proc = setter.Body.GetILProcessor();

                var ret = proc.Create(OpCodes.Ret);

                proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
                proc.Emit(OpCodes.Callvirt, context.SessionManagedPropDef.GetMethod);
                proc.Emit(OpCodes.Brtrue_S, ret);

                proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
                proc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
                proc.Emit(OpCodes.Stfld, idBackingField); // this.backingField = value;
                proc.Append(ret); // return

                LogInfo($"Successfully added updated setter for {context.IdPropDef}");
            }
        }
    }
}