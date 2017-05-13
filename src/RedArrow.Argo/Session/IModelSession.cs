using System;
using System.Collections.Generic;

namespace RedArrow.Argo.Session
{
    public interface IModelSession
    {
        bool Disposed { get; }
        Guid GetId<TModel>(TModel model);
        TAttr GetAttribute<TModel, TAttr>(TModel model, string attrName);
        void SetAttribute<TModel, TAttr>(TModel model, string attrName, TAttr value);
        TAttr GetMeta<TModel, TAttr>(TModel model, string metaName);
        void SetMeta<TModel, TAttr>(TModel model, string metaName, TAttr value);
        TRltn GetReference<TModel, TRltn>(TModel model, string attrName);
        void SetReference<TModel, TRltn>(TModel model, string attrName, TRltn value);

        IEnumerable<TElmnt> GetGenericEnumerable<TModel, TElmnt>(
            TModel model,
            string rltnName);

        IEnumerable<TElmnt> SetGenericEnumerable<TModel, TElmnt>(
            TModel model,
            string attrName,
            IEnumerable<TElmnt> value);

        ICollection<TElmnt> GetGenericCollection<TModel, TElmnt>(
            TModel model,
            string
            rltnName);

        ICollection<TElmnt> SetGenericCollection<TModel, TElmnt>(
            TModel model,
            string attrName,
            IEnumerable<TElmnt> value);
    }
}