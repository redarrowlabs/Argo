using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace RedArrow.Jsorm
{
    public partial class ModuleWeaver
    {
        private void WeaveHasOnes(ModelWeavingContext context)
        {
            var sessionGetRltnGeneric = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "GetReference");

            var sessionSetRltnGeneric = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "SetReference");

            if (sessionGetRltnGeneric == null || sessionSetRltnGeneric == null)
            {
                throw new Exception("Jsorm relationship weaving failed unexpectedly");
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
                var propAttr = propertyDef.CustomAttributes.GetAttribute(Constants.Attributes.HasOne);
                var attrName = propAttr.ConstructorArguments
                    .Where(x => x.Type == TypeSystem.String)
                    .Select(x => x.Value as string)
                    .SingleOrDefault() ?? propertyDef.Name.Camelize();

                LogInfo($"\tWeaving {propertyDef} => {attrName}");

                WeaveReferenceGetter(context, backingField, propertyDef, sessionGetRltnGeneric, attrName);
                WeaveReferenceSetter(context, backingField, propertyDef, sessionSetRltnGeneric, attrName);
            }
        }

        private void WeaveReferenceGetter(
            ModelWeavingContext context,
            FieldReference backingField,
            PropertyDefinition propertyDef,
            MethodReference sessionGetAttrGeneric,
            string attrName)
        {
            // supply generic type arguments to template
            var sessionGetAttrTyped = SupplyGenericArgs(sessionGetAttrGeneric, context.ModelTypeRef, propertyDef.GetMethod.ReturnType);

            // get
            // {
            //   if (this.__jsorm__generated_session != null)
            //   {
            //     this.<[PropName]>k__BackingField = this.__jsorm__generated_session.GetReference<[ModelType], [ReturnType]>(this.Id, "[AttrName]");
            //   }
            //   return this.<[PropName]>k__BackingField;
            // }
            propertyDef.GetMethod.Body.Instructions.Clear();
            var proc = propertyDef.GetMethod.Body.GetILProcessor();

            var returnField = proc.Create(OpCodes.Ldarg_0);

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __jsorm__generated_session field from 'this'
            proc.Emit(OpCodes.Brfalse, returnField); // if __jsorm__generated_session != null continue, else returnField

            proc.Emit(OpCodes.Ldarg_0); // load 'this' to reference backing field

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack to reference session field
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __jsorm__generated_session field from 'this'
            proc.Emit(OpCodes.Ldarg_0); // load 'this'
            proc.Emit(OpCodes.Call, context.IdPropDef.GetMethod); // invoke id property and push return onto stack
            proc.Emit(OpCodes.Ldstr, attrName); // load attrName onto stack
            proc.Emit(OpCodes.Callvirt, context.ImportReference(sessionGetAttrTyped)); // invoke session.GetAttribute(..)
            proc.Emit(OpCodes.Stfld, backingField); // store return value in 'this'.<backing field>

            proc.Append(returnField); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, backingField); // load 'this'.<backing field>
            proc.Emit(OpCodes.Ret); // return
        }

        private void WeaveReferenceSetter(
            ModelWeavingContext context,
            FieldReference backingField,
            PropertyDefinition propertyDef,
            MethodReference sessionSetAttrGeneric,
            string attrName)
        {
            // supply generic type arguments to template
            var sessionSetAttrTyped = SupplyGenericArgs(sessionSetAttrGeneric, context.ModelTypeRef, propertyDef.GetMethod.ReturnType);

            propertyDef.SetMethod.Body.Instructions.Clear();

            // set
            // {
            //     this.<[PropName]>k__BackingField = value;
            //     if (this.__jsorm__generated_session != null)
            //     {
            //         this.__jsorm__generated_session.SetReference<[ModelType], [ReturnType]>(this.Id, "[AttrName]", this.<[PropName]>k__BackingField);
            //     }
            // }
            var proc = propertyDef.SetMethod.Body.GetILProcessor();

            var ret = proc.Create(OpCodes.Ret);

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
            proc.Emit(OpCodes.Stfld, backingField); // 'this'.<backing field> = 'value'

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __jsorm__generated_session field from 'this'
            proc.Emit(OpCodes.Brfalse, ret); // if __jsorm__generated_session != null continue, else return

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack to reference session field
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __jsorm__generated_session field from 'this'
            proc.Emit(OpCodes.Ldarg_0); // load 'this'
            proc.Emit(OpCodes.Call, context.IdPropDef.GetMethod); // invoke id property and push return onto stack
            proc.Emit(OpCodes.Ldstr, attrName); // load attrName onto stack
            proc.Emit(OpCodes.Ldarg_0); // load 'this'
            proc.Emit(OpCodes.Ldfld, backingField); // load backing field
            proc.Emit(OpCodes.Callvirt, context.ImportReference(sessionSetAttrTyped)); // invoke session.SetAttribute(..)

            proc.Append(ret);
        }
    }
}