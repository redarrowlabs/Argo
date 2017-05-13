using System;
using RedArrow.Argo.Attributes;

namespace RedArrow.Argo.Client.Tests.Session.Models
{
    [Model]
    public class TestGetModelAttributeValues
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string Attribute1 { get; set; }

        [Property("attribute-2")]
        public int? Attribute2 { get; set; }

        [Property]
        public long? Attribute3 { get; set; }
    }
}