using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace WovenByFody
{
    [Model]
    public class CircularReferenceA
    {
        [Id]
        public Guid Id { get; set; }

        [HasOne(LoadStrategy.Eager)]
        public CircularReferenceB B { get; set; }
    }

    [Model]
    public class CircularReferenceB
    {
        [Id]
        public Guid Id { get; set; }
        [HasOne(LoadStrategy.Eager)]
        public CircularReferenceA A { get; set; }
        [HasOne(LoadStrategy.Eager)]
        public CircularReferenceC C { get; set; }
    }

    [Model]
    public class CircularReferenceC
    {
        [Id]
        public Guid Id { get; set; }
        [HasOne(LoadStrategy.Eager)]
        public CircularReferenceA A { get; set; }
        [HasOne(LoadStrategy.Eager)]
        public CircularReferenceD PrimaryD { get; set; }

        [HasMany]
        public IEnumerable<CircularReferenceD> AllDs { get; set; }
	}

    [Model]
    public class CircularReferenceD
    {
        [Id]
        public Guid Id { get; set; }
    }
}
