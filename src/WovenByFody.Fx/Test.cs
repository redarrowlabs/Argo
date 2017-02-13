using System;
using RedArrow.Argo.Attributes;

namespace WovenByFody
{
    [Model]
    public class Test
    {
        [Id]
        public Guid Id { get; }

        [Property]
        public string StringProp { get; set; }
    }
}
