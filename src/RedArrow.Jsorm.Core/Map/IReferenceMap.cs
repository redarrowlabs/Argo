namespace RedArrow.Jsorm.Core.Map
{
	public interface IReferenceMap
	{
		IReferenceMap Lazy();
		IReferenceMap Eager();

		IReferenceMap CascadeDelete();
	}
}
