using System;
using System.Threading.Tasks;

namespace RedArrow.Jsorm.Session
{
    public interface ISession : IDisposable
    {
	    Task<TModel> Get<TModel>(Guid id);
    }
}