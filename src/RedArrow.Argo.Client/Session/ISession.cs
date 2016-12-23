using System;
using System.Threading.Tasks;

namespace RedArrow.Argo.Client.Session
{
    public interface ISession : IDisposable
    {
        Task<TModel> Create<TModel>()
            where TModel : class;

        Task<TModel> Create<TModel>(TModel model)
            where TModel : class;
        
        Task<TModel> Get<TModel>(Guid id)
            where TModel : class;

        Task Update<TModel>(TModel model)
            where TModel : class;

        Task Delete<TModel>(TModel model)
            where TModel : class;

        Task Delete<TModel>(Guid id)
            where TModel : class;
    }
}