using System;
using System.Threading.Tasks;

namespace RedArrow.Argo.Client.Session
{
    public interface ISession : IQuerySession, IDisposable
    {
        Task<TModel> Create<TModel>();
        Task<TModel> Create<TModel>(TModel model);
        Task<TModel> Get<TModel>(Guid id);
        Task Update<TModel>(TModel model);
        Task Delete<TModel>(TModel model);
        Task Delete<TModel>(Guid id);
    }
}