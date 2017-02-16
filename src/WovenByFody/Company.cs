using System;
using RedArrow.Argo.Attributes;

namespace WovenByFody
{
    [Model("integration-test-company")]

    public class Company
    {
        [Id]
        public Guid Id { get; }

        [Property]
        public string Name { get; set; }

        [Property]
        public string Address { get; set; }

        [Property]
        public string PhoneNumber { get; set; }
    }
}
