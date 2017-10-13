using Mono.Cecil;
using Mono.Cecil.Cil;
using RedArrow.Argo.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void WeaveHasManyIds(ModelWeavingContext context)
        {
            if (_session_GetRelationshipIds == null)
            {
                throw new Exception("Argo relationship id weaving failed unexpectedly");
            }

            foreach (var propertyDef in context.MappedHasManyIds)
            {
                if (propertyDef.PropertyType.Resolve() != context.ImportReference(typeof(IEnumerable)).Resolve()
                 && propertyDef.PropertyType.Resolve() != context.ImportReference(typeof(IEnumerable<Guid>)).Resolve())
                {
                    LogError($"[HasManyIds] property must have a {typeof(IEnumerable).FullName} or {typeof(IEnumerable<Guid>).FullName} getter: {propertyDef.FullName}");
                }
                if (propertyDef.SetMethod != null)
                {
                    LogError($"[HasManyIds] property must not have a setter");
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
                var attrName = propertyDef.JsonApiName(TypeSystem, Constants.Attributes.HasManyIds);

                LogInfo($"\tWeaving {propertyDef} => {attrName}");

                WeaveRelationshipIdsGetter(context, backingField, propertyDef, attrName);
            }
        }

        private void WeaveRelationshipIdsGetter(
            ModelWeavingContext context,
            FieldReference backingField,
            PropertyDefinition refPropDef,
            string attrName)
        {
            // supply generic type arguments to template
            var sessionGetAttr = _session_GetRelationshipIds.MakeGenericMethod(context.ModelTypeDef);

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
