using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace RedArrow.Jsorm
{
	public partial class ModuleWeaver
	{
		private void AddCtor(ModelWeavingContext context)
		{
			// Ctor(Guid id, ISession session)
			// {
			//   Id = id;
			//   _jsorm_generated_session = session;
			//  }
			var ctor = new MethodDefinition(
				".ctor",
				MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
				TypeSystem.Void);

			ctor.Parameters.Add(
				new ParameterDefinition(
					"id",
					ParameterAttributes.None,
					context.ImportReference(_guidTypeDef)));
			ctor.Parameters.Add(
				new ParameterDefinition(
					"session",
					ParameterAttributes.None,
					context.SessionTypeRef));

			var objectCtor = context.ImportReference(TypeSystem.Object.Resolve().GetConstructors().First());

			var proc = ctor.Body.GetILProcessor();

			// public Patient(Guid id, ISession session)
			// {
			//	this._jsorm_generated_session = session;
			// }
			proc.Emit(OpCodes.Ldarg_0);
			proc.Emit(OpCodes.Call, objectCtor);
			proc.Emit(OpCodes.Ldarg_0);
			proc.Emit(OpCodes.Ldarg_1);
			proc.Emit(OpCodes.Callvirt, context.IdPropDef.SetMethod);
			proc.Emit(OpCodes.Ldarg_0);
			proc.Emit(OpCodes.Ldarg_2);
			proc.Emit(OpCodes.Stfld, context.SessionField);
			proc.Emit(OpCodes.Ret);

			context.Methods.Add(ctor);
		}
	}
}
