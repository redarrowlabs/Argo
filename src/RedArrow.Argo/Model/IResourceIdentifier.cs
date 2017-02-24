using System;

namespace RedArrow.Argo.Model
{
    public interface IResourceIdentifier
    {
        Guid Id { get; set; }
        string Type { get; set; }
    }
}
