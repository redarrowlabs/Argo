using System.Collections;

namespace RedArrow.Argo.Client.Collections
{
    public interface IRemoteCollection : ICollection
    {
        object Owner { get; }
        string Name { get; }

        void SetItems(IEnumerable items);
    }
}