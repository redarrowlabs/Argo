using System;
using RedArrow.Argo.Client.Model;

namespace RedArrow.Argo.Client.Session.Registry
{
    public interface IResourceRegistry
    {
        Guid StageNewResource(Type modelType, object model);
        void UnstageResource(Guid stagingId);
        void PromoteStagedResource(Guid stagingId, Guid id);
        Resource GetResource(Guid id);
    }
}
