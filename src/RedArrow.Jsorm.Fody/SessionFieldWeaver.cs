namespace RedArrow.Jsorm
{
	public partial class ModuleWeaver
	{
		private void AddSessionField(ModelWeavingContext context)
		{
			// [NonSerialized]
			// private readonly ISession __jsorm_generated_session
			context.AddSessionField(_sessionTypeDef);
		}
	}
}
