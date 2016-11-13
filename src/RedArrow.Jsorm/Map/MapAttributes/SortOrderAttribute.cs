using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RedArrow.Jsorm.Extensions;

namespace RedArrow.Jsorm.Map.MapAttributes
{
    public class SortOrderAttribute<TModel, TElement> : IMapAttribute
    {
        internal enum SortOrder
        {
            Ascending,
            Descending
        }

        private string CollectionAttribute { get; }

        private IList<string> SortAttributes { get; }
        private IDictionary<string, SortOrder> SortOrders { get; }

        private string _currentAttribute;

        public SortOrderAttribute(string collectionAttr, string elementAttr)
        {
            CollectionAttribute = collectionAttr;

            SortAttributes = new List<string>();
            SortOrders = new Dictionary<string, SortOrder>();

            ThenBy(elementAttr);
        }

        public SortOrderAttribute<TModel, TElement> ThenBy<TProp>(Expression<Func<TElement, TProp>> elementAttr)
        {
            var attrName = elementAttr.PropertyName();
            return ThenBy(attrName);
        }

        public SortOrderAttribute<TModel, TElement> ThenBy(string elementAttr)
        {
            _currentAttribute = elementAttr;

            if (!SortAttributes.Contains(_currentAttribute))
            {
                SortAttributes.Add(_currentAttribute);
                SortOrders[_currentAttribute] = SortOrder.Ascending;
            }
            return this;
        }

        public SortOrderAttribute<TModel, TElement> Ascending()
        {
            SortOrders[_currentAttribute] = SortOrder.Ascending;
            return this;
        }

        public SortOrderAttribute<TModel, TElement> Descending()
        {
            SortOrders[_currentAttribute] = SortOrder.Descending;
            return this;
        }
    }
}