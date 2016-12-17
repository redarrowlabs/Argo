using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using RedArrow.Argo.Extensions;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void WeaveHasManys(ModelWeavingContext context)
        {
            var sessionGetRltnGeneric = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "GetEnumerable");

            var sessionSetRltnGeneric = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "SetEnumerable");

            if (sessionGetRltnGeneric == null || sessionSetRltnGeneric == null)
            {
                throw new Exception("Argo relationship weaving failed unexpectedly");
            }

            foreach (var propertyDef in context.MappedHasManys)
            {
                var propertyTypeRef = propertyDef.PropertyType;
                var propertyTypeDef = propertyTypeRef.Resolve();
                if (propertyTypeDef.Interfaces.All(x => x.FullName != "System.Collections.IEnumerable") && !propertyTypeRef.HasGenericParameters)
                {
                    throw new Exception($"Argo encountered a HasMany relationship on non-IEnumerable<T> property {propertyDef.FullName}");
                }
                
                // get the backing field
                var backingField = propertyDef.BackingField();

                if (backingField == null)
                {
                    throw new Exception($"Failed to load backing field for property {propertyDef.FullName}");
                }

                // find the attrName, if there is one
                var propAttr = propertyDef.CustomAttributes.GetAttribute(Constants.Attributes.HasMany);
                var attrName = propAttr.ConstructorArguments
                    .Where(x => x.Type == TypeSystem.String)
                    .Select(x => x.Value as string)
                    .SingleOrDefault() ?? propertyDef.Name.Camelize();

                // find property generic element type
                var elementTypeDef = ((GenericInstanceType) propertyTypeRef).GenericArguments.First().Resolve();

                LogInfo($"\tWeaving {propertyDef} => {attrName}");

                WeaveEnumerableGetter(context, backingField, propertyDef, elementTypeDef, sessionGetRltnGeneric, attrName);
                WeaveEnumerableSetter(context, backingField, propertyDef, elementTypeDef, sessionSetRltnGeneric, attrName);
            }
        }

        private void WeaveEnumerableGetter(
            ModelWeavingContext context,
            FieldReference backingField,
            PropertyDefinition rltnPropDef,
            TypeDefinition elementTypeDef,
            MethodReference sessionGetRltnGeneric,
            string attrName)
        {
            // supply generic type arguments to template
	        var sessionGetRltn = sessionGetRltnGeneric.MakeGenericMethod(
		        context.ModelTypeRef,
		        rltnPropDef.PropertyType,
		        elementTypeDef);

			// get
			// {
			//   if (this.__argo__generated_session != null)
			//   {
			//     this.<[PropName]>k__BackingField = this.__argo__generated_session.GetEnumerable<[ModelType], [ReturnType]>(this.Id, "[AttrName]");
			//   }
			//   return this.<[PropName]>k__BackingField;
			// }
			rltnPropDef.GetMethod.Body.Instructions.Clear();
            var proc = rltnPropDef.GetMethod.Body.GetILProcessor();

            var returnField = proc.Create(OpCodes.Ldarg_0);

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'
			proc.Emit(OpCodes.Brfalse, returnField); // if __argo__generated_session != null continue, else returnField

			proc.Emit(OpCodes.Ldarg_0); // load 'this' to reference backing field

            proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack to reference session field
            proc.Emit(OpCodes.Ldfld, context.SessionField); // load __argo__generated_session field from 'this'
			proc.Emit(OpCodes.Ldarg_0); // load 'this'
            proc.Emit(OpCodes.Call, context.IdPropDef.GetMethod); // invoke id property and push return onto stack
            proc.Emit(OpCodes.Ldstr, attrName); // load attrName onto stack
			proc.Emit(OpCodes.Callvirt, context.ImportReference(
				sessionGetRltn,
				rltnPropDef.PropertyType.IsGenericParameter
					? context.ModelTypeRef
					: null)); // invoke session.GetEnumerable(..)
			proc.Emit(OpCodes.Stfld, backingField); // store return value in 'this'.<backing field>

            proc.Append(returnField); // load 'this' onto stack
            proc.Emit(OpCodes.Ldfld, backingField); // load 'this'.<backing field>
            proc.Emit(OpCodes.Ret); // return
        }

        private void WeaveEnumerableSetter(
            ModelWeavingContext context,
            FieldReference backingField,
            PropertyDefinition rltnPropDef,
            TypeDefinition elementTypeDef,
            MethodReference sessionSetRltnGeneric,
            string attrName)
        {
            // supply generic type arguments to template
            var sessionSetRltn = sessionSetRltnGeneric.MakeGenericMethod(
                context.ModelTypeRef,
                rltnPropDef.PropertyType,
                elementTypeDef);

            rltnPropDef.SetMethod.Body.Instructions.Clear();

			// set
			// {
			//     this.<[PropName]>k__BackingField = value;
			//     if (this.__argo__generated_session != null)
			//     {
			//         this.__argo__generated_session.SetEnumerable<[ModelType], [ReturnType]>(this.Id, "[AttrName]", this.<[PropName]>k__BackingField);
			//     }
			// }
			var proc = rltnPropDef.SetMethod.Body.GetILProcessor();

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
            proc.Emit(OpCodes.Call, context.IdPropDef.GetMethod); // invoke id property and push return onto stack
            proc.Emit(OpCodes.Ldstr, attrName); // load attrName onto stack
            proc.Emit(OpCodes.Ldarg_0); // load 'this'
            proc.Emit(OpCodes.Ldfld, backingField); // load backing field
			proc.Emit(OpCodes.Callvirt, context.ImportReference(
				sessionSetRltn,
				rltnPropDef.PropertyType.IsGenericParameter
					? context.ModelTypeRef
					: null)); // invoke session.SetEnumerable(..)

			proc.Append(ret);
        }
    }
}