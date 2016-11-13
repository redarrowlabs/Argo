namespace RedArrow.Jsorm
{
	public partial class ModuleWeaver
	{
		private void AddSessionField(ModelWeavingContext context)
		{
			// [NonSerialized]
			// private readonly ISession _jsorm_generated_session
			context.AddSessionField(_sessionTypeDef);
		}
	}
}
