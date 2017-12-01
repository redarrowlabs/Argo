using System;
using System.Text;
using Mono.Cecil;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void AddSessionField(ModelWeavingContext context)
        {
            context.SessionField = AddField(
                "session",
                _sessionTypeDef,
                FieldAttributes.Private,
                context);
        }

        private void AddIncludePathField(ModelWeavingContext context)
        {
            context.IncludePathField = AddField(
                "include",
                TypeSystem.String,
                FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly,
                context);
        }

        private static FieldDefinition AddField(
            string fieldName,
            TypeReference fieldType,
            FieldAttributes attributes,
            ModelWeavingContext context)
        {
            var fieldDef = new FieldDefinition(
                $"__argo__generated_{fieldName}",
                attributes,
                context.ImportReference(fieldType));
            context.Fields.Add(fieldDef);
            return fieldDef;
        }
    }
}