using RedArrow.Argo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace WovenByFody
{
    [Model]
    public class AllAttributes
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Name { get; set; }

        [HasOneId("singleThing")]
        public Guid SingleThingId { get; }

        [HasOne]
        public BasicModel SingleThing { get; set; }

        [HasManyIds("manyThings")]
        public IEnumerable<Guid> ManyThingIds { get; }

        [HasMany]
        public IEnumerable<BasicModel> ManyThings { get; set; }
    }
}
