using System;
using Newtonsoft.Json.Linq;
using RedArrow.Argo.Attributes;

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
    }
}