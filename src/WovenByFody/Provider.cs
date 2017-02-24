using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Session;

namespace WovenByFody
{
    [Model("integration-test-provider")]
    public class Provider
    {
        [Id]
        public Guid Id { get; protected set; }

        [Property]
        public string FirstName { get; set; }

        [Property]
        public string LastName { get; set; }

        [HasMany(LoadStrategy.Eager)]
        public ICollection<Patient> Patients { get; set; }
    }
}