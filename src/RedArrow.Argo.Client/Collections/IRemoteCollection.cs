using System.Collections;
using RedArrow.Argo.Client.Session.Patch;

namespace RedArrow.Argo.Client.Collections
{
    public interface IRemoteCollection : ICollection
    {
        bool Dirty { get; }

        object Owner { get; }
        string Name { get; }

        void SetItems(IEnumerable items);

        void Patch(PatchContext patchContext);
        void Clean();
    }
}