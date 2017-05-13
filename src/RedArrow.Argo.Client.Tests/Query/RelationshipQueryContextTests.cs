using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Query;
using WovenByFody;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Query
{
    public class RelationshipQueryContextTests
    {
        [Theory, AutoData]
        public void Ctor__Given_ModelType__Then_BasPathEqualsResourceType
            (Guid id, string rltnName)
        {
            var subject = new RelationshipQueryContext<ComplexModel, BasicModel>(id, rltnName);

            Assert.Equal($"complexModel/{id}/{rltnName}", subject.BasePath);
        }
    }
}