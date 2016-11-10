﻿namespace RedArrow.Jsorm.Core.Map.HasMany
{
    public interface ICascadeHasManyMap<TModel, TElement> : IHasManyMap<TModel, TElement>
    {
        IHasManyMap<TModel, TElement> None();

        IHasManyMap<TModel, TElement> Delete();
    }
}