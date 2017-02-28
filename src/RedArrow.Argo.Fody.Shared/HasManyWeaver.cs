using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using RedArrow.Argo.Extensions;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void WeaveHasManys(ModelWeavingContext context)
        {
            if (_session_GetGenericEnumerable == null
             || _session_SetGenericEnumerable == null
             || _session_GetGenericCollection == null
             || _session_SetGenericCollection == null)
            {
                throw new Exception("Argo relationship weaving failed unexpectedly");
            }

            foreach (var propertyDef in context.MappedHasManys)
            {
                var propertyTypeRef = propertyDef.PropertyType;
                var propertyTypeDef = propertyTypeRef.Resolve();

                MethodReference getRltnMethRef;
                MethodReference setRltnMethRef;

                if (propertyTypeDef == context.ImportReference(typeof(IEnumerable<>)).Resolve())
                {
                    getRltnMethRef = _session_GetGenericEnumerable;
                    setRltnMethRef = _session_SetGenericEnumerable;
                }
                else if (propertyTypeDef == context.ImportReference(typeof(ICollection<>)).Resolve())
                {
                    getRltnMethRef = _session_GetGenericCollection;
                    setRltnMethRef = _session_SetGenericCollection;
                }
                else
                {
                    LogError($"Argo encountered a HasMany relationship on non IEnumerable<T> or ICollection<T> property {propertyDef.FullName}");
                    continue;
                }

                // get the backing field
                var backingField = propertyDef.BackingField();

                if (backingField == null)
                {
                    LogError($"Failed to load backing field for property {propertyDef.FullName}");
                    continue;
                }

                // find the rltnName, if there is one
                var rltnName = propertyDef.JsonApiName(TypeSystem, Constants.Attributes.HasMany);

                // find property generic element type
                var elementTypeDef = ((GenericInstanceType)propertyTypeRef).GenericArguments.First().Resolve();

                LogInfo($"\tWeaving {propertyDef} => {rltnName}");

                WeaveRltnGetter(context, backingField, propertyDef, elementTypeDef, getRltnMethRef, rltnName);
                WeaveRltnSetter(context, backingField, propertyDef, elementTypeDef, setRltnMethRef, rltnName);
            }
        }

        private void WeaveRltnGetter(
            ModelWeavingContext context,
            FieldReference backingField,
            PropertyDefinition rltnPropDef,
            TypeDefinition elementTypeDef,
            MethodReference sessionGetRltnGeneric,
            string rltnName)
        {
            // supply generic type args to template
            var sessionGetRltn = sessionGetRltnGeneric.MakeGenericMethod(
                context.ModelTypeRef,
                elementTypeDef);

            rltnPropDef.GetMethod.Body.Instructions.Clear();

            var proc = rltnPropDef.GetMethod.Body.GetILProcessor();

            var endif = proc.Create(OpCodes.Ldarg_0);

            // TODO: this isn't thread safe - consider generating a Lazy in the ctor and invoking it here
            proc.Emit(OpCodes.Ldarg_0);
            proc.Emit(OpCodes.Ldfld, context.SessionField);
            proc.Emit(OpCodes.Brfalse_S, endif);
            proc.Emit(OpCodes.Ldarg_0);
            proc.Emit(OpCodes.Ldfld, backingField);
            proc.Emit(OpCodes.Brtrue_S, endif);

            proc.Emit(OpCodes.Ldarg_0);

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack to reference session field
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'
            proc.Emit(OpCodes.Ldarg_0); // load 'this'
            proc.Emit(OpCodes.Ldstr, rltnName); // load attrName onto stack
            proc.Emit(OpCodes.Callvirt, context.ImportReference(
                sessionGetRltn,
                rltnPropDef.PropertyType.IsGenericParameter
                    ? context.ModelTypeRef
                    : null)); // invoke session.GetAttribute(..)

            proc.Emit(OpCodes.Stfld, backingField); // store return value in 'this'.<backing field>

            proc.Append(endif);
            proc.Emit(OpCodes.Ldfld, backingField);
            proc.Emit(OpCodes.Ret);
        }

        private void WeaveRltnSetter(
            ModelWeavingContext context,
            FieldReference backingField,
            PropertyDefinition rltnPropDef,
            TypeDefinition elementTypeDef,
            MethodReference sessionSetRltnGeneric,
            string rltnName)
        {
            // supply generic type arguments to template
            var sessionSetRltn = sessionSetRltnGeneric.MakeGenericMethod(
                context.ModelTypeRef,
                elementTypeDef);

            rltnPropDef.SetMethod.Body.Instructions.Clear();

            // set
            // {
            //     if (this.__argo__generated_session != null)
            //     {
            //         this.<[PropName]>k__BackingField = this.__argo__generated_session.Set<[ModelType], [ElementType]>(this.Id, "[RltnName]", this.<[PropName]>k__BackingField);
            //     }
            //     else
            //     {
            //         this.<[PropName]>k__BackingField = value;
            //     }
            // }
            var proc = rltnPropDef.SetMethod.Body.GetILProcessor();

            var endif = proc.Create(OpCodes.Ldarg_0);
            var ret = proc.Create(OpCodes.Ret);

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'

            // if __argo__generated_session == null
            proc.Emit(OpCodes.Brfalse_S, endif);

            proc.Emit(OpCodes.Ldarg_0);

            proc.Emit(OpCodes.Ldarg_0);
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'
            proc.Emit(OpCodes.Ldarg_0); // load 'this'
            proc.Emit(OpCodes.Ldstr, rltnName); // load attrName onto stack
            proc.Emit(OpCodes.Ldarg_1); // load 'value'
            proc.Emit(OpCodes.Callvirt, context.ImportReference(
                sessionSetRltn,
                rltnPropDef.PropertyType.IsGenericParameter
                    ? context.ModelTypeRef
                    : null)); // invoke session.GetReference(..)

            proc.Emit(OpCodes.Stfld, backingField);

            proc.Emit(OpCodes.Br_S, ret);
            // else
            proc.Append(endif); // load 'this' onto stack to reference session field
            proc.Emit(OpCodes.Ldarg_1);
            proc.Emit(OpCodes.Stfld, backingField);

            proc.Append(ret);
        }
    }
}