using System;
using System.Linq.Expressions;

namespace RedArrow.Jsorm.Core.Map
{
	public class HasOneMap<TModel, TProp> : PropertyMap<TModel, TProp>, IReferenceMap
		where TProp : new()
	{
		public HasOneMap(Expression<Func<TModel, TProp>> property, string attrName = null) : base(property, attrName)
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
