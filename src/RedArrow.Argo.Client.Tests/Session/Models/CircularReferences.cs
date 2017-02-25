using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

namespace RedArrow.Argo.Client.Tests.Session.Models
{
    [Model]
    public class CircularReferenceA
    {
        [Id]
        public Guid Id { get; set; }

        [HasOne]
        public CircularReferenceB B { get; set; }
    }

    [Model]
    public class CircularReferenceB
    {
        [Id]
        public Guid Id { get; }

        [HasOne]
        public CircularReferenceA A { get; set; }
        [HasOne]
        public CircularReferenceC C { get; set; }
    }

    [Model]
    public class CircularReferenceC
    {
        [Id]
        public Guid Id { get; }

        [HasOne]
        public CircularReferenceA A { get; set; }

        [HasOne]
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
