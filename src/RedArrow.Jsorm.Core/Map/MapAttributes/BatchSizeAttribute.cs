namespace RedArrow.Jsorm.Core.Map.MapAttributes
{
	public class BatchSizeAttribute : IMapAttribute
	{
		private int BatchSize { get; }

		public BatchSizeAttribute(int batchSize)
		{
			BatchSize = batchSize;
		}
	}
}
