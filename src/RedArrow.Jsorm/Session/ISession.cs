using System;

namespace RedArrow.Jsorm.Session
{
    public interface ISession : IDisposable
    {
	    object GetAttribute();

	    //TAttr GetAttribute<TModel, TAttr>(Guid id, string attrName);
    }
}