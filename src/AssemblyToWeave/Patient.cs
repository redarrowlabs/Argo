using System;

namespace AssemblyToWeave
{
    public class Patient
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public Provider Provider { get; set; }
    }
}