using System;
using System.Collections.Generic;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Model;

namespace GeneratedOverrides
{
    [Model]
    public class Overrides
    {
        [Id]
        public Guid Id { get; set; }

        [Resource]
        public IDictionary<string, object> Resource { get; set; }
        
        [Patch]
        public Resource Patch { get; set; }
    }
}
