using Mono.Cecil;
using Mono.Cecil.Cil;
using RedArrow.Argo.Extensions;
using System;
using System.Linq;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void WeaveHasOneIds(ModelWeavingContext context)
        {
            if (_session_GetReferenceId == null)
            {
                throw new Exception("Argo relationship id weaving failed unexpectedly");
            }

            foreach (var propertyDef in context.MappedHasOneIds)
            {
                if(propertyDef.PropertyType.Resolve() != context.ImportReference(typeof(Guid)).Resolve())
                {
                    LogError($"[HasOneId] property must have a {typeof(Guid).FullName} getter: {propertyDef.FullName}");
                }
                if(propertyDef.SetMethod != null)
                {
                    LogError($"[HasOneId] property must not have a setter");
                }

                // get the backing field
                var backingField = propertyDef
                    ?.GetMethod
                    ?.Body
                    ?.Instructions
                    ?.SingleOrDefault(x => x.OpCode == OpCodes.Ldfld)
                    ?.Operand as FieldReference;

                if (backingField == null)
                {
                    throw new Exception($"Failed to load backing field for property {propertyDef.FullName}");
                }

                // find the attrName, if there is one
                var attrName = propertyDef.JsonApiName(TypeSystem, Constants.Attributes.HasOneId);

                LogInfo($"\tWeaving {propertyDef} => {attrName}");

                WeaveReferenceIdGetter(context, backingField, propertyDef, attrName);
            }
        }

        private void WeaveReferenceIdGetter(
            ModelWeavingContext context,
            FieldReference backingField,
            PropertyDefinition refPropDef,
            string attrName)
        {
            // supply generic type arguments to template
            var sessionGetAttr = _session_GetReferenceId.MakeGenericMethod(context.ModelTypeDef);

            // get
            // {
            //   if (this.__argo__generated_session != null)
            //   {
            //     this.<[PropName]>k__BackingField = this.__argo__generated_session.GetReference<[ModelType], [ReturnType]>(this, "[AttrName]");
            //   }
            //   return this.<[PropName]>k__BackingField;
            // }
            refPropDef.GetMethod.Body.Instructions.Clear();
            var proc = refPropDef.GetMethod.Body.GetILProcessor();

            var returnField = proc.Create(OpCodes.Ldarg_0);

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'
            proc.Emit(OpCodes.Brfalse, returnField); // if __argo__generated_session != null continue, else returnField

            proc.Emit(OpCodes.Ldarg_0); // load 'this' to reference backing field

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack to reference session field
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'
            proc.Emit(OpCodes.Ldarg_0); // load 'this'
            proc.Emit(OpCodes.Ldstr, attrName); // load attrName onto stack
            proc.Emit(OpCodes.Callvirt, context.ImportReference(
                sessionGetAttr,
                refPropDef.PropertyType.IsGenericParameter
                    ? context.ModelTypeDef
                    : null)); // invoke session.GetReference(..)
            proc.Emit(OpCodes.Stfld, backingField); // store return value in 'this'.<backing field>

            proc.Append(returnField); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, backingField); // load 'this'.<backing field>
            proc.Emit(OpCodes.Ret); // return
        }
    }
}
