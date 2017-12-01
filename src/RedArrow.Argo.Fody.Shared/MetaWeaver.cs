using Mono.Cecil;
using Mono.Cecil.Cil;
using RedArrow.Argo.Extensions;
using System;
using System.Linq;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void WeaveMeta(ModelWeavingContext context)
        {
            // get a generic method template from session type
            var sessionSetMetaGeneric = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "SetMeta");

            if (sessionSetMetaGeneric == null)
            {
                throw new Exception("Argo meta weaving failed unexpectedly");
            }

            foreach (var propertyDef in context.MappedMeta)
            {
                if (propertyDef.CustomAttributes.ContainsAttribute(Constants.Attributes.Property))
                {
                    LogError($"Property {propertyDef.FullName} cannot be included in both attributes and meta");
                }
            }

            foreach (var propertyDef in context.MappedMeta.Where(prop => prop.SetMethod != null))
            {
                // get the backing field
                var backingField = propertyDef.BackingField();

                if (backingField == null)
                {
                    throw new Exception($"Failed to load backing field for property {propertyDef.FullName}");
                }

                // find the attrName, if there is one
                var metaName = propertyDef.JsonApiName(TypeSystem, Constants.Attributes.Meta);

                LogInfo($"\tWeaving {propertyDef} => {metaName}");

                WeaveMetaSetter(context, backingField, propertyDef, sessionSetMetaGeneric, metaName);
            }
        }

        private void WeaveMetaSetter(
            ModelWeavingContext context,
            FieldReference backingField,
            PropertyDefinition metaPropDef,
            MethodReference sessionSetMetaGeneric,
            string metaName)
        {
            // supply generic type arguments to template
            var sessionSetAttr = sessionSetMetaGeneric
                .MakeGenericMethod(context.ModelTypeDef, metaPropDef.PropertyType);

            metaPropDef.SetMethod.Body.Instructions.Clear();

            // set
            // {
            //     if (this.__argo__generated_session != null && this.<[PropName]>k__BackingField != value)
            //     {
            //         this.__argo__generated_session.SetRelationship<[ModelType], [ReturnType]>(this, "[MetaName]", value);
            //     }
            //     this.<[PropName]>k__BackingField = value;
            // }
            var proc = metaPropDef.SetMethod.Body.GetILProcessor();

            var endif = proc.Create(OpCodes.Ldarg_0);

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'
            proc.Emit(OpCodes.Brfalse_S, endif); // if __argo__generated_session != null continue, else return

            EmitInequalityCheck(context, proc, backingField, endif);

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack to reference session field
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'
            proc.Emit(OpCodes.Ldarg_0); // load 'this'
            proc.Emit(OpCodes.Ldstr, metaName); // load metaName onto stack
            proc.Emit(OpCodes.Ldarg_1); // load 'value'
            proc.Emit(OpCodes.Callvirt, context.ImportReference(
                sessionSetAttr,
                metaPropDef.PropertyType.IsGenericParameter
                    ? context.ModelTypeDef
                    : null)); // invoke session.GetMeta(..)

            proc.Append(endif); // load 'this' onto stack
            proc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
            proc.Emit(OpCodes.Stfld, backingField); // 'this'.<backing field> = 'value'

            proc.Emit(OpCodes.Ret); // return
        }
    }
}