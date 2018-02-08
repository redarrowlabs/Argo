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
            if (_session_GetReference == null)
            {
                throw new Exception("Argo relationship weaving failed unexpectedly");
            }

            foreach (var propertyDef in context.MappedHasOnes)
            {
                // get the backing field
                var backingField = propertyDef.BackingField();

                if (backingField == null)
                {
                    throw new Exception($"Failed to load backing field for property {propertyDef.FullName}");
                }

                /* Add a boolean field to track whether the init was attempted.  A null check
                 * on the backing field is not good enough since they could have set the value to null. */
                var backingFieldInitialized = AddField(
                    backingField.Name + "Initialized",
                    TypeSystem.Boolean,
                    FieldAttributes.Private,
                    context);

                // find the attrName, if there is one
                var attrName = propertyDef.JsonApiName(TypeSystem, Constants.Attributes.HasOne);

                LogInfo($"\tWeaving {propertyDef} => {attrName}");

                WeaveReferenceGetter(context, backingField, backingFieldInitialized, propertyDef, attrName);
                if (propertyDef.SetMethod == null) return;
                WeaveReferenceSetter(backingField, backingFieldInitialized, propertyDef);
            }
        }

        private void WeaveReferenceGetter(
            ModelWeavingContext context,
            FieldReference backingField,
            FieldReference backingFieldInitialized,
            PropertyDefinition refPropDef,
            string attrName)
        {
            // supply generic type arguments to template
            var sessionGetAttr = _session_GetReference.MakeGenericMethod(context.ModelTypeDef, refPropDef.PropertyType);

            // get
            // {
            //   if (this.__argo__generated_session != null && !this.<[PropName]>k__BackingFieldInitialized)
            //   {
            //     this.<[PropName]>k__BackingField = this.__argo__generated_session.GetReference<[ModelType], [ReturnType]>(this, "[AttrName]");
            //     this.<[PropName]>k__BackingFieldInitialized = true;
            //   }
            //   return this.<[PropName]>k__BackingField;
            // }
            refPropDef.GetMethod.Body.Instructions.Clear();
            var proc = refPropDef.GetMethod.Body.GetILProcessor();

            var returnField = proc.Create(OpCodes.Ldarg_0);

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'
            proc.Emit(OpCodes.Brfalse, returnField); // if __argo__generated_session != null continue, else returnField

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, backingFieldInitialized); // load <[PropName]>k__BackingFieldInitialized from 'this'
            proc.Emit(OpCodes.Brtrue, returnField); // !this.<[PropName]>k__BackingFieldInitialized continue, else returnField

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

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldc_I4_1); // load true (1) onto stack
            proc.Emit(OpCodes.Stfld, backingFieldInitialized); // store true in 'this'.<backing field>Initialized

            proc.Append(returnField); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, backingField); // load 'this'.<backing field>
            proc.Emit(OpCodes.Ret); // return
        }

        private void WeaveReferenceSetter(
            FieldReference backingField,
            FieldReference backingFieldInitialized,
            PropertyDefinition refPropDef)
        {
            refPropDef.SetMethod.Body.Instructions.Clear();

            // set
            // {
            //     this.<[PropName]>k__BackingField = value;
            //     this.<[PropName]>k__BackingFieldInitialized = true;
            // }
            var proc = refPropDef.SetMethod.Body.GetILProcessor();

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
            proc.Emit(OpCodes.Stfld, backingField); // 'this'.<backing field> = 'value'

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldc_I4_1); // load true (1) onto stack
            proc.Emit(OpCodes.Stfld, backingFieldInitialized); // store true in 'this'.<backing field>Initialized

            proc.Emit(OpCodes.Ret); // return
        }
    }
}