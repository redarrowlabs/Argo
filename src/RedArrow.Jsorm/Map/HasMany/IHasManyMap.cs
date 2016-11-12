using System;
using System.Linq.Expressions;

namespace RedArrow.Jsorm.Map.HasMany
{
    public interface IHasManyMap<TModel, TElement>
    {
        ISortedHasManyMap<TModel, TElement> SortBy<TProp>(Expression<Func<TElement, TProp>> elementAttr);

        ISortedHasManyMap<TModel, TElement> SortBy(string elementAttr);

        IHasManyMap<TModel, TElement> BatchSize(int batchSize);

        IHasManyMap<TModel, TElement> Lazy();

        IHasManyMap<TModel, TElement> Eager();

        ICascadeHasManyMap<TModel, TElement> Cascade();
    }
}