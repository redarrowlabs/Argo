using System;
using RedArrow.Argo.Attributes;

namespace WovenByFody
{
    [Model]
    public class ModelWithNoIdSetter
    {
        [Id]
        public Guid Id { get; }
    }

    [Model]
    public class ModelWithPublicIdSetter
    {
        [Id]
        public Guid Id { get; set; }
    }

    [Model]
    public class ModelWithPrivateIdSetter
    {
        [Id]
        public Guid Id { get; private set; }
    }
}