using System;
using RedArrow.Jsorm.Map.Id.Generator;

namespace RedArrow.Jsorm.Map.Id
{
    public interface IIdMap<TModel> : IPropertyMap
    {
        IIdMap<TModel> GeneratedBy(Func<Guid> generator);
    }
}