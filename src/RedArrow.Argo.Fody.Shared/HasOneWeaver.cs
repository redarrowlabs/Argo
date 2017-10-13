using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using RedArrow.Argo.Extensions;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void WeaveHasOnes(ModelWeavingContext context)
        {
            if (_session_GetReference == null || _session_SetReference == null)
            {
                throw new Exception("Argo relationship weaving failed unexpectedly");
            }

            foreach (var propertyDef in context.MappedHasOnes)
            {
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
                var attrName = propertyDef.JsonApiName(TypeSystem, Constants.Attributes.HasOne);

                LogInfo($"\tWeaving {propertyDef} => {attrName}");

                WeaveReferenceGetter(context, backingField, propertyDef, attrName);
                if (propertyDef.SetMethod == null) return; 
                WeaveReferenceSetter(context, backingField, propertyDef, attrName);
            }
        }

        private void WeaveReferenceGetter(
            ModelWeavingContext context,
            FieldReference backingField,
            PropertyDefinition refPropDef,
            string attrName)
        {
            // supply generic type arguments to template
            var sessionGetAttr = _session_GetReference.MakeGenericMethod(context.ModelTypeDef, refPropDef.PropertyType);

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

        private void WeaveReferenceSetter(
            ModelWeavingContext context,
            FieldReference backingField,
            PropertyDefinition refPropDef,
            string attrName)
        {
            // supply generic type arguments to template
            var sessionSetAttr = _session_SetReference.MakeGenericMethod(context.ModelTypeDef, refPropDef.PropertyType);

            refPropDef.SetMethod.Body.Instructions.Clear();

            // set
            // {
            //     this.<[PropName]>k__BackingField = value;
            //     if (this.__argo__generated_session != null)
            //     {
            //         this.__argo__generated_session.SetReference<[ModelType], [ReturnType]>(this, "[AttrName]", this.<[PropName]>k__BackingField);
            //     }
            // }
            var proc = refPropDef.SetMethod.Body.GetILProcessor();

            var ret = proc.Create(OpCodes.Ret);

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
            proc.Emit(OpCodes.Stfld, backingField); // 'this'.<backing field> = 'value'

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'
            proc.Emit(OpCodes.Brfalse, ret); // if __argo__generated_session != null continue, else return

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack to reference session field
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'
            proc.Emit(OpCodes.Ldarg_0); // load 'this'
            proc.Emit(OpCodes.Ldstr, attrName); // load attrName onto stack
            proc.Emit(OpCodes.Ldarg_0); // load 'this'
            proc.Emit(OpCodes.Ldfld, backingField); // load backing field
            proc.Emit(OpCodes.Callvirt, context.ImportReference(
                sessionSetAttr,
                refPropDef.PropertyType.IsGenericParameter
                    ? context.ModelTypeDef
                    : null)); // invoke session.GetReference(..)

            proc.Append(ret);
        }
    }
}