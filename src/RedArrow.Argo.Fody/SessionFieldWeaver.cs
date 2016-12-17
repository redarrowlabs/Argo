namespace RedArrow.Argo
{
	public partial class ModuleWeaver
	{
		private void AddSessionField(ModelWeavingContext context)
		{
			// [NonSerialized]
			// private readonly ISession __argo__generated_session
			context.AddSessionField(_sessionTypeDef);
		}
	}
}
