using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Query;

namespace RedArrow.Argo.Client.Session
{
    public interface IQuerySession
    {
        Task<IEnumerable<TModel>> Query<TModel>(IQueryContext query = null);
        IQueryable<TModel> CreateQuery<TModel>();

        IEnumerable<TRltn> GetRelated<TParent, TRltn>(
            Guid id,
            Expression<Func<TParent, IEnumerable<TRltn>>> relationship);
    }
}