using System;
using System.Linq.Expressions;
using RedArrow.Jsorm.Core.Map.Attributes;

namespace RedArrow.Jsorm.Core.Map.HasMany
{
	public interface IHasManyMap<TModel, TElement>
		where TElement : new()
	{
		ISortedHasManyMap<TModel, TElement> SortBy<TProp>(Expression<Func<TElement, TProp>> elementAttr);
		ISortedHasManyMap<TModel, TElement> SortBy(string elementAttr);
		IHasManyMap<TModel, TElement> BatchSize(int batchSize);
		IHasManyMap<TModel, TElement> Lazy();
		IHasManyMap<TModel, TElement> Eager();
		ICascadeHasManyMap<TModel, TElement> Cascade();
	}
}