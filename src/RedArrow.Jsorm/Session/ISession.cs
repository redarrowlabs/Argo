using System;
using System.Threading.Tasks;

namespace RedArrow.Jsorm.Session
{
    public interface ISession : IDisposable
    {
        Task<TModel> Create<TModel>(TModel model);

        Task<TModel> Get<TModel>(Guid id);

	    Task Delete<TModel>(TModel model);
	    Task Delete<TModel>(Guid id);
    }
}