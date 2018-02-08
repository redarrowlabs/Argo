using System.Collections;

namespace RedArrow.Argo.Client.Collections
{
    public interface IRemoteCollection : ICollection
    {
        object Owner { get; }
        string Name { get; }

        /// <summary>
        /// Theoretically, the relationships in this collection match the remote collection
        /// </summary>
        bool IsModified { get; }

        void SetItems(IEnumerable items);
    }
}