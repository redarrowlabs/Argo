using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Query;

namespace RedArrow.Argo.Client.Session
{
    public interface IQuerySession
    {
        IQueryable<TModel> CreateQuery<TModel>();
        Task<IEnumerable<TModel>> Query<TModel>(IQueryContext query = null);
    }
}
