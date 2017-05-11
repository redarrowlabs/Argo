using RedArrow.Argo.Attributes;
using System;

namespace WovenByFody
{
    [Model("integration-test-patient")]
    public class Patient
    {
        [Id]
        public Guid Id { get; }

        [Property]
        public string FirstName { get; set; }

        [Property]
        public string LastName { get; set; }

        [Property]
        public int Age { get; set; }

        [Property]
        public decimal AccountBalance { get; set; }

        [HasOne(LoadStrategy.Eager)]
        public Provider Provider { get; set; }

        [HasOne(LoadStrategy.Eager)]
        public Company Insurance { get; set; }

        [Meta]
        public DateTime Created { get; set; }

        [Meta]
        public string Version { get; set; }
    }
}