using System.Linq;
using Mono.Cecil;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private TypeDefinition _sessionTypeDef;
        private TypeDefinition _guidTypeDef;

        private int _stringComparison_ordinal;
        private MethodDefinition _string_equals;
        private TypeDefinition _equalityComparerTypeDef;
	    private MethodDefinition _object_equals;

        private void LoadTypeDefinitions()
        {
            var argoAssemblyDef = AssemblyResolver.Resolve("RedArrow.Argo");
            _sessionTypeDef = argoAssemblyDef.MainModule.GetType("RedArrow.Argo.Session.IModelSession");

            var msCoreAssemblyDef = AssemblyResolver.Resolve("mscorlib");
            _guidTypeDef = msCoreAssemblyDef.MainModule.GetType("System.Guid");

            _string_equals = ModuleDefinition
                .TypeSystem
                .String
                .Resolve()
                .Methods
                .First(x => x.IsStatic &&
                            x.Name == "Equals" &&
                            x.Parameters.Count == 3 &&
                            x.Parameters[0].ParameterType.Name == "String" &&
                            x.Parameters[1].ParameterType.Name == "String" &&
                            x.Parameters[2].ParameterType.Name == "StringComparison");

            _stringComparison_ordinal = (int) msCoreAssemblyDef.MainModule.GetType("System.StringComparison")
                .Fields
                .First(x => x.Name == "Ordinal")
                .Constant;

            _equalityComparerTypeDef = msCoreAssemblyDef.MainModule.GetType("System.Collections.Generic.EqualityComparer`1");

	        _object_equals = ModuleDefinition
		        .TypeSystem
		        .Object
		        .Resolve()
		        .Methods
		        .First(x => x.Name == "Equals" && x.Parameters.Count == 2);
        }
    }
}