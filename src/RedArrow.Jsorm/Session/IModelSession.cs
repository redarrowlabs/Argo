using System;

namespace RedArrow.Jsorm.Session
{
    public interface IModelSession
    {
        TAttr GetAttribute<TModel, TAttr>(Guid id, string attrName)
            where TModel : class;

        void SetAttribute<TModel, TAttr>(Guid id, string attrName, TAttr value)
            where TModel : class;

		TRltn GetRelationship<TModel, TRltn>(Guid id, string attrName)
			where TModel : class
			where TRltn : class;

		void SetRelationship<TModel, TRltn>(Guid id, string attrName, TRltn rltn)
			where TModel : class
			where TRltn : class;
	}
}