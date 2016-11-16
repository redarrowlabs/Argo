using RedArrow.Jsorm.Attributes;
using System;

namespace AssemblyToWeave
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

		[Property]
		public string FavoriteFood { get; set; }

        public Provider Provider { get; set; }
    }
}