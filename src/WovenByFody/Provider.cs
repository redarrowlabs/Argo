using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;

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

        [HasMany]
        public IEnumerable<Patient> Patients { get; set; }
    }
}