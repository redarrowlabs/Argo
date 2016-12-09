using System;
using System.Collections;
using System.Collections.Generic;

namespace RedArrow.Jsorm.Session
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

        TRltn GetEnumerable<TModel, TRltn, TElmnt>(Guid id, string attrName)
            where TModel : class
            where TRltn : IEnumerable<TElmnt>
            where TElmnt : class;

        void SetEnumerable<TModel, TRltn, TElmnt>(Guid id, string attrName, TRltn value)
            where TModel : class
            where TRltn : IEnumerable<TElmnt>
            where TElmnt : class;
    }
}