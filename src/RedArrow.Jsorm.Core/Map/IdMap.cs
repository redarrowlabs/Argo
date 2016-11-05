using System;
using System.Linq.Expressions;

namespace RedArrow.Jsorm.Core.Map
{
	public class IdMap<TModel, TId> : PropertyMap<TModel, TId>, IIdMap
	{
		public IdMap(Expression<Func<TModel, TId>> property) : base(property, null)
		{
		}
	}
}
