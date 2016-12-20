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

        IEnumerable<TElmnt> GetGenericEnumerable<TModel, TElmnt>(Guid id, string rltnName)
            where TModel : class
            where TElmnt : class;

        IEnumerable<TElmnt> SetGenericEnumerable<TModel, TElmnt>(Guid id, string attrName, IEnumerable<TElmnt> value)
            where TModel : class
            where TElmnt : class;

        ICollection<TElmnt> GetGenericCollection<TModel, TElmnt>(Guid id, string rltnName)
            where TModel : class
            where TElmnt : class;

        ICollection<TElmnt> SetGenericCollection<TModel, TElmnt>(Guid id, string attrName, IEnumerable<TElmnt> value)
            where TModel : class
            where TElmnt : class;
    }
}