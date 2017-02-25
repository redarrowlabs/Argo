using System;
using System.Collections.Generic;

namespace RedArrow.Argo.Session
{
    public interface IModelSession
    {
        bool Disposed { get; }

        Guid GetId<TModel>(TModel model);

        TAttr GetAttribute<TModel, TAttr>(TModel model, string attrName)
            where TModel : class;

        void SetAttribute<TModel, TAttr>(TModel model, string attrName, TAttr value)
            where TModel : class;

        TRltn GetReference<TModel, TRltn>(TModel model, string attrName)
            where TModel : class
            where TRltn : class;

        void SetReference<TModel, TRltn>(TModel model, string attrName, TRltn value)
            where TModel : class
            where TRltn : class;

        IEnumerable<TElmnt> GetGenericEnumerable<TModel, TElmnt>(TModel model, string rltnName)
            where TModel : class
            where TElmnt : class;

        IEnumerable<TElmnt> SetGenericEnumerable<TModel, TElmnt>(TModel model, string attrName, IEnumerable<TElmnt> value)
            where TModel : class
            where TElmnt : class;

        ICollection<TElmnt> GetGenericCollection<TModel, TElmnt>(TModel model, string rltnName)
            where TModel : class
            where TElmnt : class;

        ICollection<TElmnt> SetGenericCollection<TModel, TElmnt>(TModel model, string attrName, IEnumerable<TElmnt> value)
            where TModel : class
            where TElmnt : class;
    }
}