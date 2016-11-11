using RedArrow.Jsorm.Core.Extensions;
using RedArrow.Jsorm.Core.Map.MapAttributes;
using RedArrow.Jsorm.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RedArrow.Jsorm.Core.Map.HasMany
{
    public class HasManyMap<TModel, TElement> :
        PropertyMap<TModel, IEnumerable<TElement>>,
        ISortedHasManyMap<TModel, TElement>,
        ICascadeHasManyMap<TModel, TElement>,
        IHasManyMap<TModel, TElement> // for brevity
    {
        public HasManyMap(Expression<Func<TModel, IEnumerable<TElement>>> property, string attrName = null) :
            base(property, attrName)
        {
        }

        public override void Configure(ISessionFactory factory)
        {
            throw new NotImplementedException();
        }

        public ISortedHasManyMap<TModel, TElement> SortBy<TProp>(Expression<Func<TElement, TProp>> elementAttr)
        {
            var attrName = elementAttr.PropertyName();
            return SortBy(attrName);
        }

        public ISortedHasManyMap<TModel, TElement> SortBy(string elementAttr)
        {
            if (elementAttr == null)
            {
                throw new ArgumentNullException(nameof(elementAttr));
            }

            MapAttributes[nameof(SortOrderAttribute<TModel, TElement>)] = new SortOrderAttribute<TModel, TElement>(AttributeName, elementAttr);
            return this;
        }

        public ISortedHasManyMap<TModel, TElement> ThenBy<TProp>(Expression<Func<TElement, TProp>> elementAttr)
        {
            var attr = MapAttributes[nameof(SortOrderAttribute<TModel, TElement>)] as SortOrderAttribute<TModel, TElement>;
            attr?.ThenBy(elementAttr);
            return this;
        }

        public ISortedHasManyMap<TModel, TElement> ThenBy(string elementAttr)
        {
            var attr = MapAttributes[nameof(SortOrderAttribute<TModel, TElement>)] as SortOrderAttribute<TModel, TElement>;
            attr?.ThenBy(elementAttr);
            return this;
        }

        public ISortedHasManyMap<TModel, TElement> Ascending()
        {
            var attr = MapAttributes[nameof(SortOrderAttribute<TModel, TElement>)] as SortOrderAttribute<TModel, TElement>;
            attr?.Ascending();
            return this;
        }

        public ISortedHasManyMap<TModel, TElement> Descending()
        {
            var attr = MapAttributes[nameof(SortOrderAttribute<TModel, TElement>)] as SortOrderAttribute<TModel, TElement>;
            attr?.Descending();
            return this;
        }

        public IHasManyMap<TModel, TElement> BatchSize(int batchSize)
        {
            if (batchSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize));
            }

            MapAttributes[nameof(BatchSizeAttribute)] = new BatchSizeAttribute(batchSize);
            return this;
        }

        public IHasManyMap<TModel, TElement> Lazy()
        {
            MapAttributes[nameof(FetchStrategyAttribute)] = FetchStrategyAttribute.Lazy;
            return this;
        }

        public IHasManyMap<TModel, TElement> Eager()
        {
            MapAttributes[nameof(FetchStrategyAttribute)] = FetchStrategyAttribute.Eager;
            return this;
        }

        public ICascadeHasManyMap<TModel, TElement> Cascade()
        {
            MapAttributes[nameof(CascadeStyleAttribute)] = CascadeStyleAttribute.All;
            return this;
        }

        public IHasManyMap<TModel, TElement> None()
        {
            MapAttributes[nameof(CascadeStyleAttribute)] = CascadeStyleAttribute.None;
            return this;
        }

        public IHasManyMap<TModel, TElement> Delete()
        {
            MapAttributes[nameof(CascadeStyleAttribute)] = CascadeStyleAttribute.Delete;
            return this;
        }
    }
}