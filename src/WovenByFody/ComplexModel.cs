using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace WovenByFody
{
    [Model]
    public class ComplexModel
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string PropertyA { get; set; }

        [Property]
        public string PropertyB { get; set; }

        [HasOne]
        public BasicModel PrimaryBasicModel { get; set; }

        [HasMany]
        public IEnumerable<BasicModel> BasicModels { get; set; }
    }
}