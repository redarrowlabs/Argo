using System;
using RedArrow.Argo.Attributes;

namespace RedArrow.Argo.Client.Tests.Session.Registry
{
    [Model]
    public class TestWovenModel
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string CamelizedProperty { get; set; }

        [Property("customized-property")]
        public string CustomizedProperty { get; set; }
    }
}
