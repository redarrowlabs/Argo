using System;

namespace RedArrow.Jsorm.Session
{
    public interface IModelSession
    {
        TAttr GetAttribute<TModel, TAttr>(Guid id, string attrName)
            where TModel : class;

        void SetAttribute<TModel, TAttr>(Guid id, string attrName, TAttr value)
            where TModel : class;
    }
}