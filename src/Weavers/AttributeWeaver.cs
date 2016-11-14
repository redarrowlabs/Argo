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
				throw new Exception("Jsorm attribute weaving failed unexpectedly");
			}

			foreach (var propertyDef in context.MappedAttributes)
			{
				LogInfo($"\tWeaving {propertyDef.Name}");

				// find the attrName, if there is one
				var propAttr = propertyDef.CustomAttributes.GetAttribute(Constants.Attributes.Property);
				var attrName = propAttr.ConstructorArguments.SingleOrDefault().Value as string ?? propertyDef.Name;

				WeaveGetter(context, propertyDef, sessionGetAttrGeneric, attrName);
				WeaveSetter(context, propertyDef, sessionSetAttrGeneric, attrName);
			}
		}

		private void WeaveGetter(
			ModelWeavingContext context,
			PropertyDefinition propertyDef,
			MethodReference sessionGetAttrGeneric,
			string attrName)
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
			proc.Emit(OpCodes.Ldfld, context.SessionField); // load __jsorm__generated_session field from 'this'
			proc.Emit(OpCodes.Ldarg_0); // load 'this'
			proc.Emit(OpCodes.Call, context.IdPropDef.GetMethod); // invoke id property and push return onto stack
			proc.Emit(OpCodes.Ldstr, attrName); // load attrName onto stack
			proc.Emit(OpCodes.Callvirt, context.ImportReference(sessionGetAttrTyped)); // invoke session.GetAttribute(..)
			proc.Emit(OpCodes.Ret); // return
		}

		private void WeaveSetter(
			ModelWeavingContext context,
			PropertyDefinition propertyDef,
			MethodReference sessionSetAttrGeneric,
			string attrName)
		{
			// supply generic type arguments to template
			var sessionSetAttrTyped = SupplyGenericArgs(context, propertyDef, sessionSetAttrGeneric);
			
			propertyDef.SetMethod.Body.Instructions.Clear();
			var proc = propertyDef.SetMethod.Body.GetILProcessor();

			// set
			// {
			//	  return this._jsorm_generated_session.SetAttribute<Patient, string>(this.Id, "[attrName]", value);
			// }
			proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
			proc.Emit(OpCodes.Ldfld, context.SessionField); // load __jsorm__generated_session field from 'this'
			proc.Emit(OpCodes.Ldarg_0); // load 'this'
			proc.Emit(OpCodes.Call, context.IdPropDef.GetMethod); // invoke id property and push return onto stack
			proc.Emit(OpCodes.Ldstr, attrName); // load attrName onto stack
			proc.Emit(OpCodes.Ldarg_1); // load 'value' onto stack
			proc.Emit(OpCodes.Callvirt, context.ImportReference(sessionSetAttrTyped)); // invoke session.SetAttribute(...)
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
