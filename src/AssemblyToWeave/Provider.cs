using RedArrow.Jsorm.Attributes;
using System;
using System.Collections.Generic;

namespace AssemblyToWeave
{
    [Model("integration-test-provider")]
    public class Provider
    {
        [Id]
        public Guid Id { get; protected set; }

        [Property("first-name")]
        public string FirstName { get; set; }

        [Property("last-name")]
        public string LastName { get; set; }

        public IEnumerable<Patient> Patients { get; set; }
    }
}