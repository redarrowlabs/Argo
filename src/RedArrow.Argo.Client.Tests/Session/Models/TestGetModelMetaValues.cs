using System;
using RedArrow.Argo.Attributes;

namespace RedArrow.Argo.Client.Tests.Session.Models
{
    [Model]
    public class TestGetModelMetaValues
    {
        [Id]
        public Guid Id { get; set; }

        [Meta]
        public string Meta1 { get; set; }

        [Meta("meta-2")]
        public int? Meta2 { get; set; }

        [Meta]
        public long? Meta3 { get; set; }
    }
}
