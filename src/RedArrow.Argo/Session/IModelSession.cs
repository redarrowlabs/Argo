using System;
using System.Collections.Generic;

namespace RedArrow.Argo.Session
{
    public interface IModelSession
    {
        bool Disposed { get; }
        Guid GetId<TModel>(TModel model);
        TAttr GetAttribute<TModel, TAttr>(TModel model, string attrName);
        TAttr GetMeta<TModel, TAttr>(TModel model, string metaName);
        Guid GetReferenceId<TModel>(TModel model, string attrName);
        TRltn GetReference<TModel, TRltn>(TModel model, string attrName);

        IEnumerable<Guid> GetRelationshipIds<TModel>(TModel model, string rltnName);
        IEnumerable<TElmnt> GetGenericEnumerable<TModel, TElmnt>(
            TModel model,
            string rltnName);

        ICollection<TElmnt> GetGenericCollection<TModel, TElmnt>(
            TModel model,
            string
            rltnName);
    }
}