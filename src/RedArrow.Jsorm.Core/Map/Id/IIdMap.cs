using RedArrow.Jsorm.Core.Map.Id.Generator;

namespace RedArrow.Jsorm.Core.Map.Id
{
    public interface IIdMap<TModel> : IPropertyMap
    {
        IIdMap<TModel> GeneratedBy(IIdentifierGenerator generator);
    }
}