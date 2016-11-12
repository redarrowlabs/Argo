using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace RedArrow.Jsorm
{
    public partial class ModuleWeaver
    {
        private void AddModelCtors()
        {
            foreach (var keyValuePair in _modelsToMaps)
            {
	            var modelTypeDef = keyValuePair.Key;
	            var mapTypeDef = keyValuePair.Value;

				var importedSessionTypeRef = modelTypeDef.Module.ImportReference(_sessionTypeRef);
				
				// add ISession field
				var sessionField = new FieldDefinition(
					"_jsorm_generated_session",
					FieldAttributes.Private | FieldAttributes.NotSerialized | FieldAttributes.InitOnly,
					importedSessionTypeRef);
				modelTypeDef.Fields.Add(sessionField);

				// add Ctor(ISession session){_jsorm_generated_session = session;}
				var ctor = new MethodDefinition(
					".ctor",
					MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
					TypeSystem.Void);

				ctor.Parameters.Add(new ParameterDefinition("session", ParameterAttributes.None, importedSessionTypeRef));
				var objectCtor = modelTypeDef.Module.ImportReference(TypeSystem.Object.Resolve().GetConstructors().First());

				var proc = ctor.Body.GetILProcessor();
				proc.Emit(OpCodes.Ldarg_0);
				proc.Emit(OpCodes.Call, objectCtor);
				proc.Emit(OpCodes.Ldarg_0);
				proc.Emit(OpCodes.Ldarg_1);
				proc.Emit(OpCodes.Stfld, sessionField);
				proc.Emit(OpCodes.Ret);

				modelTypeDef.Methods.Add(ctor);
				
				WeaveAttributes(modelTypeDef, mapTypeDef, sessionField);
			}
        }
    }
}