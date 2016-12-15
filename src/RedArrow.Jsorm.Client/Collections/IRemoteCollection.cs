using System.Collections.Generic;

namespace RedArrow.Jsorm.Client.Collections
{
    public interface IRemoteCollection<T> : ICollection<T>
        where T : class
    {
        bool Dirty { get; }

        object Owner { get; }
        string Name { get; }
        
        void Initialize(IEnumerable<T> items);
    }
}
