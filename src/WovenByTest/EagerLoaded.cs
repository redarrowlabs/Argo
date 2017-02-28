using System;
using RedArrow.Argo.Attributes;

namespace WovenByTest
{
    [Model]
    public class EagerLoadedA
    {
        [Id]
        public Guid Id { get; set; }

        [HasOne(LoadStrategy.Eager)]
        public EagerLoadedB B { get; set; }
    }

    [Model]
    public class EagerLoadedB
    {
        [Id]
        public Guid Id { get; set; }

        [HasOne(LoadStrategy.Eager)]
        public EagerLoadedA A { get; set; }

        [HasOne(LoadStrategy.Eager)]
        public EagerLoadedC C { get; set; }
    }

    [Model]
    public class EagerLoadedC
    {
        [Id]
        public Guid Id { get; set; }

        [HasOne(LoadStrategy.Eager)]
        public EagerLoadedA A { get; set; }
    }
}
