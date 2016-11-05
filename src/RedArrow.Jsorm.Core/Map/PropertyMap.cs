using System;
using System.Linq.Expressions;
using RedArrow.Jsorm.Core.Extensions;

namespace RedArrow.Jsorm.Core.Map
{
	public abstract class PropertyMap<TModel, TProp> : IPropertyMap
	{
		public Type PropertyType { get; }
		public string PropertyName { get; }

		public string AttributeName { get; }

		protected PropertyMap(Expression<Func<TModel, TProp>> expression, string attrName = null)
		{
			PropertyType = expression.ReturnType;
			PropertyName = expression.PropertyName();

			AttributeName = attrName ?? PropertyName;
		}
	}
}
