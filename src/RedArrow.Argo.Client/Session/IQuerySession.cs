using System.Collections.Generic;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Query;

namespace RedArrow.Argo.Client.Session
{
    public interface IQuerySession
    {
        Task<IEnumerable<TModel>> Query<TModel>(QueryContext query = null);
    }
}
