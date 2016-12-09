using System.Linq;
using Mono.Cecil;

namespace RedArrow.Jsorm
{
    public partial class ModuleWeaver
    {
        private TypeDefinition _sessionTypeDef;
        private TypeDefinition _guidTypeDef;

        private int _stringComparison_ordinal;
        private MethodReference _string_equals;

        private TypeDefinition _equalityComparer;

        private void LoadTypeDefinitions()
        {
            var jsormAssemblyDef = AssemblyResolver.Resolve("RedArrow.Jsorm");
            _sessionTypeDef = jsormAssemblyDef.MainModule.GetType("RedArrow.Jsorm.Session.IModelSession");

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

            _equalityComparer = msCoreAssemblyDef.MainModule.GetType("System.Collections.Generic.EqualityComparer`1");
        }
    }
}