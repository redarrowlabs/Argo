using RedArrow.Argo.Attributes;
using System;

namespace WovenByFody
{
    [Model]
    public class Widget
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Sku { get; set; }

        [Meta("__system")]
        public SystemMetadata Metadata { get; set; }
    }
}