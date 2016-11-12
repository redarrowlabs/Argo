using Mono.Cecil;

namespace RedArrow.Jsorm
{
    public partial class ModuleWeaver
    {
        private void AddModelCtors()
        {
            foreach (var model in _modelToMap.Keys)
            {
                AddSessionField(model);
                AddConstructor(model);
            }
        }

        private void AddSessionField(TypeDefinition type)
        {
            ModuleDefinition.ImportReference(_sessionTypeRef);
            var sessionField = new FieldDefinition(
                "_jsorm_generated_session",
                FieldAttributes.Private | FieldAttributes.NotSerialized,
                _sessionTypeRef);
            type.Fields.Add(sessionField);
        }

        private void AddConstructor(TypeDefinition type)
        {
            var method = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, TypeSystem.Void);
            method.Parameters.Add(new ParameterDefinition(JsormAssembly.MainModule.GetType("RedArrow.Jsorm.Session.ISession")));
        }
    }
}