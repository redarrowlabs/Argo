using Mono.Cecil;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void AddSessionField(ModelWeavingContext context)
        {
            context.SessionField = AddField("session", _sessionTypeDef, context);
        }

        private void AddIncludePathField(ModelWeavingContext context)
        {
            context.IncludePathField = AddField("includePath", TypeSystem.String, context);
        }

        private static FieldDefinition AddField(string fieldName, TypeReference fieldType, ModelWeavingContext context)
        {
            var fieldDef = new FieldDefinition(
                    $"__argo__generated_{fieldName}",
                    FieldAttributes.Private,
                    context.ImportReference(fieldType));

            context.Fields.Add(fieldDef);
            return fieldDef;
        }
    }
}