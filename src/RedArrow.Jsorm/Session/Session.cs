using System;

namespace RedArrow.Jsorm.Session
{
    public class Session : IModelSession, ISession
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

	    public TAttr GetAttribute<TModel, TAttr>(Guid id, string attrName)
	    {
		    throw new NotImplementedException();
	    }

	    public void SetAttribute<TModel, TAttr>(Guid id, string attrName, TAttr value)
	    {
		    throw new NotImplementedException();
	    }
    }
}