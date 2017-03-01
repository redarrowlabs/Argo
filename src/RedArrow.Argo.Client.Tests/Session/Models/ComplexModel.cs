using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedArrow.Argo.Attributes;

namespace RedArrow.Argo.Client.Tests.Session.Models
{
    [Model]
    public class ComplexModel
    {
        [Id]
        public Guid Id { get; set; }

        [Property]
        public string PropertyA { get; set; }

        [Property]
        public string PropertyB { get; set; }

        [HasOne]
        public BasicModel PrimaryBasicModel { get; set; }

        [HasMany]
        public IEnumerable<BasicModel> BasicModels { get; set; }
    }
}
