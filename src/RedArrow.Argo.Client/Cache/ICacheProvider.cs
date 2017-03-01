using System;
using System.Collections.Generic;

namespace RedArrow.Argo.Client.Cache
{
    public interface ICacheProvider
    {
        IEnumerable<object> Items { get; }

        void Update(Guid id, object model);
		TModel Retrieve<TModel>(Guid id)
			where TModel : class;
		void Remove(Guid id);
    }
}
