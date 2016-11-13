namespace RedArrow.Jsorm
{
	public partial class ModuleWeaver
	{
		private void AddSessionField(ModelWeavingContext context)
		{
			// [NonSerialized]
			// private readonly ISession __jsorm__generated_session
			context.AddSessionField(_sessionTypeDef);
		}
	}
}
