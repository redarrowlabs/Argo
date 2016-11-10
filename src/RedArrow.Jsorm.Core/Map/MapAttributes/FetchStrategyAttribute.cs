namespace RedArrow.Jsorm.Core.Map.MapAttributes
{
	public class FetchStrategyAttribute : IMapAttribute
	{
		private static readonly object  LazySyncRoot = new object();
		private static FetchStrategyAttribute _lazy;
		public static FetchStrategyAttribute Lazy
		{
			get
			{
				if (_lazy == null)
				{
					lock (LazySyncRoot)
					{
						if (_lazy == null)
						{
							_lazy = new FetchStrategyAttribute(FetchStrategy.Lazy);
						}
					}
				}
				return _lazy;
			}
		}


		private static readonly object EagerSyncRoot = new object();
		private static FetchStrategyAttribute _eager;
		public static FetchStrategyAttribute Eager
		{
			get
			{
				if (_eager == null)
				{
					lock (EagerSyncRoot)
					{
						if (_eager == null)
						{
							_eager = new FetchStrategyAttribute(FetchStrategy.Eager);
						}
					}
				}
				return _eager;
			}
		}

		internal enum FetchStrategy
		{
			Lazy,
			Eager
		}

		private FetchStrategy Strategy { get; }

		internal FetchStrategyAttribute(FetchStrategy strategy)
		{
			Strategy = strategy;
		}
	}
}
