namespace RedArrow.Jsorm.Core.Map.Id
{
	public interface IIdMap : IPropertyMap
	{
		IIdMap GeneratedBy(IIdentifierGenerator generator);
	}
}
