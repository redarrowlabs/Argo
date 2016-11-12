using System;

namespace RedArrow.Jsorm.Session
{
    public class Session : ISession
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

	    public object GetAttribute()
	    {
		    return "Hello World";
	    }
    }
}