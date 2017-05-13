using System;
using RedArrow.Argo.Attributes;

namespace WovenByFody
{
    [Model]
    public class BasicModel
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string PropA { get; set; }

        [Property]
        public string PropB { get; set; }

        [Property]
        public string[] PropBs { get; set; }
    }
}