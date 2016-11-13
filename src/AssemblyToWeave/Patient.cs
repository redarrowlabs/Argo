using System;

namespace AssemblyToWeave
{
    public class Patient
    {
        public Guid Id { get; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

	    public int Age { get; set; }

	    public decimal AccountBalance { get; set; }

        public Provider Provider { get; set; }
    }
}