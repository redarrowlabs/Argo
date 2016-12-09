﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil.Rocks;
using RedArrow.Jsorm.Extensions;

namespace RedArrow.Jsorm
{
    public partial class ModuleWeaver
    {
        private void WeaveAttributes(ModelWeavingContext context)
        {
            // get a generic method template from session type
            var sessionGetAttrGeneric = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "GetAttribute");

            var sessionSetAttrGeneric = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "SetAttribute");

            if (sessionGetAttrGeneric == null || sessionSetAttrGeneric == null)
            {
                throw new Exception("Jsorm attribute weaving failed unexpectedly");
            }

            foreach (var propertyDef in context.MappedAttributes)
            {
                // get the backing field
                var backingField = propertyDef.BackingField();

                if (backingField == null)
                {
                    throw new Exception($"Failed to load backing field for property {propertyDef?.FullName}");
                }

                // find the attrName, if there is one
                var propAttr = propertyDef.CustomAttributes.GetAttribute(Constants.Attributes.Property);
                var attrName = propAttr.ConstructorArguments
                    .Select(x => x.Value as string)
                    .SingleOrDefault() ?? propertyDef.Name.Camelize();

                LogInfo($"\tWeaving {propertyDef} => {attrName}");

                WeaveAttrGetter(backingField, propertyDef);
                WeaveAttrSetter(context, backingField, propertyDef, sessionSetAttrGeneric, attrName);
            }
        }

        private static void WeaveAttrGetter(
            FieldReference backingField,
            PropertyDefinition propertyDef)
        {
            // get
            // {
            //   return this.<[PropName]>k__BackingField;
            // }
            propertyDef.GetMethod.Body.Instructions.Clear();
            var proc = propertyDef.GetMethod.Body.GetILProcessor();
			
            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, backingField); // load 'this'.<backing field>
            proc.Emit(OpCodes.Ret); // return
        }

        private void WeaveAttrSetter(
            ModelWeavingContext context,
            FieldReference backingField,
            PropertyDefinition attrPropDef,
            MethodReference sessionSetAttrGeneric,
            string attrName)
        {
			// supply generic type arguments to template
			var sessionSetAttr = sessionSetAttrGeneric.MakeGenericMethod(context.ModelTypeRef, attrPropDef.PropertyType);

            attrPropDef.SetMethod.Body.Instructions.Clear();

			// set
			// {
			//     if (this.__jsorm__generated_session != null && this.<[PropName]>k__BackingField != value)
			//     {
			//         this.__jsorm__generated_session.SetRelationship<[ModelType], [ReturnType]>(this.Id, "[AttrName]", value);
			//     }
			//     this.<[PropName]>k__BackingField = value;
			// }
			var proc = attrPropDef.SetMethod.Body.GetILProcessor();

            var endif = proc.Create(OpCodes.Ldarg_0);

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __jsorm__generated_session field from 'this'
            proc.Emit(OpCodes.Brfalse_S, endif); // if __jsorm__generated_session != null continue, else return

	        EmitEqualityCheck(context, proc, backingField, endif);

	        proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack to reference session field
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __jsorm__generated_session field from 'this'
            proc.Emit(OpCodes.Ldarg_0); // load 'this'
            proc.Emit(OpCodes.Call, context.IdPropDef.GetMethod); // invoke id property and push return onto stack
            proc.Emit(OpCodes.Ldstr, attrName); // load attrName onto stack
            proc.Emit(OpCodes.Ldarg_1); // load 'value'
			proc.Emit(OpCodes.Callvirt, context.ImportReference(
				sessionSetAttr,
				attrPropDef.PropertyType.IsGenericParameter
					? context.ModelTypeRef
					: null)); // invoke session.GetAttribute(..)

			proc.Append(endif); // load 'this' onto stack
			proc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
			proc.Emit(OpCodes.Stfld, backingField); // 'this'.<backing field> = 'value'

			proc.Emit(OpCodes.Ret); // return
        }

        private void EmitEqualityCheck(ModelWeavingContext context, ILProcessor proc, FieldReference backingField, Instruction endif)
        {
            var returnType = backingField.FieldType;

            if (returnType == TypeSystem.String)
            {
                proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
                proc.Emit(OpCodes.Ldfld, backingField); // load __jsorm__generated_session field from 'this'
                proc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
                proc.Emit(OpCodes.Ldc_I4, _stringComparison_ordinal); // load 'StringComparison.Ordinal' onto stack
                proc.Emit(OpCodes.Call, context.ImportReference(_string_equals));
            }
            else
            {
                // TODO: find equality method
	            var typeEqualityMethRef = returnType.EqualityOperator();
                if (typeEqualityMethRef == null)
                {
                    if (returnType.SupportsCeq() && returnType.IsValueType)
                    {
                        proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
                        proc.Emit(OpCodes.Ldfld, backingField); // load __jsorm__generated_session field from 'this'
                        proc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
                        proc.Emit(OpCodes.Ceq); // compare 'this'.<backing field> == 'value' and push result onto stack
                    }
                    else if (returnType.IsValueType && _equalityComparerTypeDef != null)
                    {
                        var equalityComparerTypeDef = _equalityComparerTypeDef.Resolve();

                        var returnTypeComparer = context.ImportReference(equalityComparerTypeDef.MakeGenericInstanceType(returnType));
                        var getDefaultMethRef = context.ImportReference(equalityComparerTypeDef
                            .Properties
                            .Single(prop => prop.Name == "Default").GetMethod);
                        var equalsMethRef = context.ImportReference(equalityComparerTypeDef
                            .Methods
                            .Single(meth => meth.Name == "Equals" && meth.Parameters.Count == 2));

                        getDefaultMethRef.DeclaringType = returnTypeComparer;
                        equalsMethRef.DeclaringType = returnTypeComparer;

						proc.Emit(OpCodes.Nop);
                        proc.Emit(OpCodes.Call, getDefaultMethRef);
                        proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
                        proc.Emit(OpCodes.Ldfld, backingField); // load __jsorm__generated_session field from 'this'
                        proc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
                        proc.Emit(OpCodes.Callvirt, equalsMethRef);
                    }
					else if (returnType.IsValueType || returnType.IsGenericParameter)
					{
						proc.Emit(OpCodes.Ldarg_0);
						proc.Emit(OpCodes.Ldfld, backingField);
						proc.Emit(OpCodes.Box, returnType);
						proc.Emit(OpCodes.Ldarg_1);
						proc.Emit(OpCodes.Box, returnType);
						proc.Emit(OpCodes.Call, context.ImportReference(_object_equals));
					}
					else
					{
						proc.Emit(OpCodes.Ldarg_0);
						proc.Emit(OpCodes.Ldfld, backingField);
						proc.Emit(OpCodes.Ldarg_1);
						proc.Emit(OpCodes.Call, context.ImportReference(_object_equals));
					}
                }
                else
                {
                    proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
                    proc.Emit(OpCodes.Ldfld, backingField); // load __jsorm__generated_session field from 'this'
                    proc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
                    proc.Emit(OpCodes.Call, context.ImportReference(typeEqualityMethRef));
                }
            }

            proc.Emit(OpCodes.Brtrue_S, endif);
        }
    }
}