using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace RedArrow.Jsorm
{
	public partial class ModuleWeaver
	{
		private void VerifyIdProperty(ModelWeavingContext context)
		{
			// if id property wasn't mapped...
			if (context.IdPropDef == null)
			{
				throw new Exception($"Model {context.ModelType.FullName} does not have an id property mapped");
			}

			// if id property doesn't have a setter, try to add one
			if (context.IdPropDef != null && context.IdPropDef.SetMethod == null)
			{
				var getterBackingField = context
					.IdPropDef
					?.GetMethod
					?.Body
					?.Instructions
					?.SingleOrDefault(x => x.OpCode == OpCodes.Ldfld)
					?.Operand as FieldReference;

				if (getterBackingField != null)
				{
					var setter = new MethodDefinition(
						$"set_{context.IdPropDef.Name}",
						MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
						context.ImportReference(TypeSystem.Void));

					setter.Parameters.Add(
						new ParameterDefinition(
							"value",
							ParameterAttributes.None,
							context.IdPropDef.PropertyType));
					setter.SemanticsAttributes = MethodSemanticsAttributes.Setter;

					var proc = setter.Body.GetILProcessor();
					proc.Emit(OpCodes.Ldarg_0);
					proc.Emit(OpCodes.Ldarg_1);
					proc.Emit(OpCodes.Stfld, getterBackingField);
					proc.Emit(OpCodes.Ret);

					context.Methods.Add(setter);

					context.IdPropDef.SetMethod = setter;
				}
				else
				{
					throw new Exception($"Model {context.ModelType.FullName} id property '{context.IdPropDef?.Name}' has no setter. This property must have a private or protected setter");
				}
			}

			// if id property setter was defined public - denied!
			if (context.IdPropDef != null && context.IdPropDef.SetMethod.IsPublic)
			{
				throw new Exception($"Model {context.ModelType.FullName} id property '{context.IdPropDef?.Name}' is public. This property must have a private or protected setter");
			}
		}
	}
}
