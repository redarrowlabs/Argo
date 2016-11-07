using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RedArrow.Jsorm.Core.Extensions;

namespace RedArrow.Jsorm.Core.Map.Attributes
{
	public class SortOrderAttribute<TModel> : IMapAttribute
	{
		internal enum SortOrder
		{
			Ascending,
			Descending
		}

		private IList<string> SortAttributes { get; }
		private IDictionary<string, SortOrder> SortOrders { get; }

		private string _currentAttribute;

		public SortOrderAttribute(string attrName)
		{
			SortAttributes = new List<string>();
			SortOrders = new Dictionary<string, SortOrder>();

			ThenBy(attrName);
		}

		public SortOrderAttribute<TModel> ThenBy<TProp>(Expression<Func<TModel, TProp>> attribute)
		{
			var attrName = attribute.PropertyName();
			return ThenBy(attrName);
		}

		public SortOrderAttribute<TModel> ThenBy(string attrName)
		{
			_currentAttribute = attrName;

			if (!SortAttributes.Contains(_currentAttribute))
			{
				SortAttributes.Add(_currentAttribute);
			}
			SortOrders[_currentAttribute] = SortOrder.Ascending;
			return this;
		}

		public SortOrderAttribute<TModel> Ascending()
		{
			SortOrders[_currentAttribute] = SortOrder.Ascending;
			return this;
		}

		public SortOrderAttribute<TModel> Descending()
		{
			SortOrders[_currentAttribute] = SortOrder.Descending;
			return this;
		}
	}

}
