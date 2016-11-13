using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

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
				throw new Exception("TODO");
			}

			foreach (var propAttrMap in context.MappedAttributes)
			{
				// find the property
				var propertyDef = context.Properties.SingleOrDefault(x => x.Name == propAttrMap.Key);
				if (propertyDef == null)
				{
					throw new Exception($"Jsorm failed to weave mapped property {propAttrMap.Key} on model {context.ModelType.FullName}");
				}

				WeaveGetter(context, propertyDef, sessionGetAttrGeneric, propAttrMap);
				WeaveSetter(context, propertyDef, sessionSetAttrGeneric, propAttrMap);
			}
		}

		private void WeaveGetter(
			ModelWeavingContext context,
			PropertyDefinition propertyDef,
			MethodReference sessionGetAttrGeneric,
			KeyValuePair<string, string> propAttrMap)
		{
			// supply generic type arguments to template
			var sessionGetAttrTyped = SupplyGenericArgs(context, propertyDef, sessionGetAttrGeneric);

			propertyDef.GetMethod.Body.Instructions.Clear();
			var proc = propertyDef.GetMethod.Body.GetILProcessor();

			// get
			// {
			//	  return this._jsorm_generated_session.GetAttribute<Patient, string>(this.Id, "[attrName]");
			// }
			proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
			proc.Emit(OpCodes.Ldfld, context.SessionField); // load _jsorm_generated_session field from 'this'
			proc.Emit(OpCodes.Ldarg_0); // load 'this'
			proc.Emit(OpCodes.Call, context.IdPropDef.GetMethod); // invoke id property and push return onto stack
			proc.Emit(OpCodes.Ldstr, propAttrMap.Value); // load attrName onto stack
			proc.Emit(OpCodes.Callvirt, context.ImportReference(sessionGetAttrTyped)); // invoke session.GetAttribute(..)
			proc.Emit(OpCodes.Ret); // return
		}

		private void WeaveSetter(
			ModelWeavingContext context,
			PropertyDefinition propertyDef,
			MethodReference sessionSetAttrGeneric,
			KeyValuePair<string, string> propAttrMap)
		{
			// supply generic type arguments to template
			var sessionGetAttrTyped = SupplyGenericArgs(context, propertyDef, sessionSetAttrGeneric);
			
			propertyDef.SetMethod.Body.Instructions.Clear();
			var proc = propertyDef.SetMethod.Body.GetILProcessor();

			// set
			// {
			//	  return this._jsorm_generated_session.SetAttribute<Patient, string>(this.Id, "[attrName]", value);
			// }
			proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
			proc.Emit(OpCodes.Ldfld, context.SessionField); // load _jsorm_generated_session field from 'this'
			proc.Emit(OpCodes.Ldarg_0); // load 'this'
			proc.Emit(OpCodes.Call, context.IdPropDef.GetMethod); // invoke id property and push return onto stack
			proc.Emit(OpCodes.Ldstr, propAttrMap.Value); // load attrName onto stack
			proc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
			proc.Emit(OpCodes.Callvirt, context.ImportReference(sessionGetAttrTyped)); // invoke session.SetAttribute(...)
			proc.Emit(OpCodes.Ret); // return
		}

		private GenericInstanceMethod SupplyGenericArgs(
			ModelWeavingContext context,
			PropertyDefinition propertyDef,
			MethodReference methodRef)
		{
			var genericMethod = new GenericInstanceMethod(methodRef);
			genericMethod.GenericArguments.Add(context.ModelTypeRef);
			genericMethod.GenericArguments.Add(propertyDef.GetMethod.ReturnType);
			return genericMethod;
		}
	}
}
