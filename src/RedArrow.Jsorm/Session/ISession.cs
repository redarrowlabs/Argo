using System;

namespace RedArrow.Jsorm.Session
{
    public interface ISession : IDisposable
    {
	    TAttr GetAttribute<TModel, TAttr>(Guid id, string attrName);
		void SetAttribute<TModel, TAttr>(Guid id, string attrName, TAttr value);
	}
}