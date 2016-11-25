using Mono.Cecil;

namespace RedArrow.Jsorm
{
    public partial class ModuleWeaver
    {
        private TypeDefinition _sessionTypeDef;
        private TypeDefinition _guidTypeDef;
        private TypeDefinition _ienumerableTypeDef;

        private void LoadTypeDefinitions()
        {
            var jsormAssemblyDef = AssemblyResolver.Resolve("RedArrow.Jsorm");
            _sessionTypeDef = jsormAssemblyDef.MainModule.GetType("RedArrow.Jsorm.Session.IModelSession");

            var msCoreAssemblyDef = AssemblyResolver.Resolve("mscorlib");
            _guidTypeDef = msCoreAssemblyDef.MainModule.GetType("System.Guid");

            _ienumerableTypeDef = msCoreAssemblyDef.MainModule.GetType("System.Collections.IEnumerable");
        }
    }
}