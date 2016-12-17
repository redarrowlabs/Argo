using System;
using System.Collections.Generic;

namespace RedArrow.Argo.Session
{
    public interface IModelSession
    {
        TAttr GetAttribute<TModel, TAttr>(Guid id, string attrName)
            where TModel : class;

        void SetAttribute<TModel, TAttr>(Guid id, string attrName, TAttr value)
            where TModel : class;

        TRltn GetReference<TModel, TRltn>(Guid id, string attrName)
            where TModel : class
            where TRltn : class;

        void SetReference<TModel, TRltn>(Guid id, string attrName, TRltn value)
            where TModel : class
            where TRltn : class;

        TEnum GetEnumerable<TModel, TEnum, TRltn>(Guid id, string attrName)
            where TModel : class
            where TEnum : IEnumerable<TRltn>
            where TRltn : class;

        void SetEnumerable<TModel, TEnum, TRltn>(Guid id, string attrName, TEnum value)
            where TModel : class
            where TEnum : IEnumerable<TRltn>
            where TRltn : class;
    }
}