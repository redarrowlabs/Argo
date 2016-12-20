using Mono.Cecil;

namespace RedArrow.Argo
{
    public partial class ModuleWeaver
    {
        private void AddSessionField(ModelWeavingContext context)
        {
            // [NonSerialized]
            // private readonly ISession __jsorm__generated_session
            context.SessionField = new FieldDefinition(
                    "__argo__generated_session",
                    FieldAttributes.Private | FieldAttributes.NotSerialized,
                    context.ImportReference(_sessionTypeDef));

            context.Fields.Add(context.SessionField);

            // TODO: foreach HasMany, add a lazyField = Lazy<>(() => _session.GetCollection(...))
        }
    }
}