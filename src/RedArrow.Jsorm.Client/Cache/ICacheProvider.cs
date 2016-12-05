using System;

namespace RedArrow.Jsorm.Client.Cache
{
    public interface ICacheProvider
    {
        void Update(Guid id, object model);
        object Retrieve(Guid id);
        void Remove(Guid id);
    }
}
