using System.Linq;
using Mono.Cecil;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private TypeDefinition _sessionTypeDef;
        private MethodDefinition _session_GetGenericEnumerable;
        private MethodDefinition _session_SetGenericEnumerable;
        private MethodDefinition _session_GetGenericCollection;
        private MethodDefinition _session_SetGenericCollection;

        private TypeDefinition _guidTypeDef;
        private TypeDefinition _genericIEnumerableTypeDef;
        private TypeDefinition _genericICollectionTypeDef;

        private int _stringComparison_ordinal;
        private MethodDefinition _string_equals;
        private TypeDefinition _equalityComparerTypeDef;
        private MethodDefinition _object_equals;

        private void LoadTypeDefinitions()
        {
            var jsormAssemblyDef = AssemblyResolver.Resolve("RedArrow.Argo");
            _sessionTypeDef = jsormAssemblyDef.MainModule.GetType("RedArrow.Argo.Session.IModelSession");

            _session_GetGenericEnumerable = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "GetGenericEnumerable");
            _session_SetGenericEnumerable = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "SetGenericEnumerable");
            _session_GetGenericCollection = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "GetGenericCollection");
            _session_SetGenericCollection = _sessionTypeDef
                .Methods
                .SingleOrDefault(x => x.Name == "SetGenericCollection");

            var msCoreAssemblyDef = AssemblyResolver.Resolve("mscorlib");
            _guidTypeDef = msCoreAssemblyDef.MainModule.GetType("System.Guid");
            _genericIEnumerableTypeDef = msCoreAssemblyDef.MainModule.GetType("System.Collections.Generic.IEnumerable`1");
            _genericICollectionTypeDef = msCoreAssemblyDef.MainModule.GetType("System.Collections.Generic.ICollection`1");

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

            _stringComparison_ordinal = (int)msCoreAssemblyDef.MainModule.GetType("System.StringComparison")
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