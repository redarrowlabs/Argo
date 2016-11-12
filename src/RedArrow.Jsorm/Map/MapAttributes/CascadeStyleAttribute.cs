namespace RedArrow.Jsorm.Map.MapAttributes
{
	public class CascadeStyleAttribute : IMapAttribute
	{
		private static readonly object AllSyncRoot = new object();
		private static CascadeStyleAttribute _all;
		public static CascadeStyleAttribute All
		{
			get
			{
				if (_all == null)
				{
					lock (AllSyncRoot)
					{
						if (_all == null)
						{
							_all = new CascadeStyleAttribute(CascadeStyle.All);
						}
					}
				}
				return _all;
			}
		}

		private static readonly object NoneSyncRoot = new object();
		private static CascadeStyleAttribute _none;
		public static CascadeStyleAttribute None
		{
			get
			{
				if (_none == null)
				{
					lock (NoneSyncRoot)
					{
						if (_none == null)
						{
							_none = new CascadeStyleAttribute(CascadeStyle.None);
						}
					}
				}
				return _none;
			}
		}

		private static readonly object DeleteSyncRoot = new object();
		private static CascadeStyleAttribute _delete;
		public static CascadeStyleAttribute Delete
		{
			get
			{
				if (_delete == null)
				{
					lock (DeleteSyncRoot)
					{
						if (_delete == null)
						{
							_delete = new CascadeStyleAttribute(CascadeStyle.Delete);
						}
					}
				}
				return _delete;
			}
		}

		internal enum CascadeStyle
		{
			All,
			None,
			Delete
		}

		private CascadeStyle Cascade { get; }

		internal CascadeStyleAttribute(CascadeStyle cascade)
		{
			Cascade = cascade;
		}
	}
}
