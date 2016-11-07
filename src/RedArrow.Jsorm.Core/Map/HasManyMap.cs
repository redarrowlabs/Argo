using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RedArrow.Jsorm.Core.Map
{
	public class HasManyMap<TModel, TElement> : PropertyMap<TModel, IEnumerable<TElement>>, IReferenceMap
		where TElement : new()
	{
		public HasManyMap(Expression<Func<TModel, IEnumerable<TElement>>> property, string attrName = null) :
			base(property, attrName)
		{
		}

		public IReferenceMap Lazy()
		{
			throw new NotImplementedException();
		}

		public IReferenceMap Eager()
		{
			throw new NotImplementedException();
		}

		public IReferenceMap CascadeDelete()
		{
			throw new NotImplementedException();
		}
	}
}
