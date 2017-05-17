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

        [Property("info.name")]
        public string Name { get; set; }

        [Property("info.stuff")]
        public string Stuff { get; set; }

        [Meta]
        public string Whatever { get; set; }

        [Meta("system.createdAt")]
        public DateTime CreatedAt { get; private set; }

        [Meta("system.updatedAt")]
        public DateTime UpdatedAt { get; private set; }

        [Meta("system.eTag")]
        public string ETag { get; private set; }
    }
}