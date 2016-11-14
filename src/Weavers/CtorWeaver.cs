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
			// Ctor(Guid id, IModelSession session)
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

			// public Patient(Guid id, IModelSession session)
			// {
			//	this._jsorm_generated_session = session;
			// }
			proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
			proc.Emit(OpCodes.Call, objectCtor); // call base ctor on 'this'
			proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
			proc.Emit(OpCodes.Ldarg_1); // load 'id' onto stack
			proc.Emit(OpCodes.Callvirt, context.IdPropDef.SetMethod); // this.Id = id;
			proc.Emit(OpCodes.Ldarg_0); // load 'this' onto stack
			proc.Emit(OpCodes.Ldarg_2); // load 'session' onto stack
			proc.Emit(OpCodes.Stfld, context.SessionField); // this.__jsorm__generated_session = session;
			proc.Emit(OpCodes.Ret); // return
			
			context.Methods.Add(ctor);
		}
	}
}
