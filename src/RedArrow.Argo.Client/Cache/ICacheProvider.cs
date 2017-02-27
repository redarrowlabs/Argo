using System;

namespace RedArrow.Argo.Client.Cache
{
    public interface ICacheProvider
    {
        void Update(Guid id, object model);
		TModel Retrieve<TModel>(Guid id)
			where TModel : class;
		void Remove(Guid id);
    }
}
