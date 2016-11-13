using System;
using System.Collections.Generic;

namespace AssemblyToWeave
{
    public class Provider
    {
        public Guid Id { get; protected set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public IEnumerable<Patient> Patients { get; set; }
    }
}