using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RedArrow.Jsorm.Core.Map
{
	public class HasManyMap<TModel, TElement> : PropertyMap<TModel, IEnumerable<TElement>>, IHasManyMap
		where TElement : new()
	{
		public HasManyMap(Expression<Func<TModel, IEnumerable<TElement>>> property, string attrName = null) : base(property, attrName)
		{
		}
	}
}
