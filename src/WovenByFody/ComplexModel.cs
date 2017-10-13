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

        [HasOneId("primaryBasicModel")]
        public Guid PrimaryBasicModelId { get; }

        [HasOne]
        public BasicModel PrimaryBasicModel { get; set; }

        [HasManyIds("basicModels")]
        public IEnumerable<Guid> BasicModelIds { get; }

        [HasMany]
        public IEnumerable<BasicModel> BasicModels { get; set; }
    }
}