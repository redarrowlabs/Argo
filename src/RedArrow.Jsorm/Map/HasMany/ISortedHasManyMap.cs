using System;
using System.Linq.Expressions;

namespace RedArrow.Jsorm.Map.HasMany
{
    public interface ISortedHasManyMap<TModel, TElement> : IHasManyMap<TModel, TElement>
    {
        ISortedHasManyMap<TModel, TElement> ThenBy<TProp>(Expression<Func<TElement, TProp>> elementAttr);

        ISortedHasManyMap<TModel, TElement> ThenBy(string elementAttr);

        ISortedHasManyMap<TModel, TElement> Ascending();

        ISortedHasManyMap<TModel, TElement> Descending();
    }
}