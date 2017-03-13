using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Linq.Extensions;
using RedArrow.Argo.TestUtils;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Linq.Integration
{
    public class QueryWhereTests : IntegrationTest
    {
        public QueryWhereTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper) :
            base(fixture, outputHelper)
        {
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task Where__When_SimpleStringExpression__Then_ReturnMatchingResults
            (string[] props)
        {
            await DeleteAll<AllPropertyTypes>();

            using (var session = SessionFactory.CreateSession())
            {
                await Task.WhenAll(props
                    .Select((x, i) => session.Create(new AllPropertyTypes {StringProperty = props[i]}))
                    .ToArray());
            }

            using (var session = SessionFactory.CreateSession())
            {
                var expectedProp = props[1];
                var results = session.CreateQuery<AllPropertyTypes>()
                    .Where(x => x.StringProperty == expectedProp)
                    .ToArray();

                Assert.Equal(1, results.Length);
                Assert.Equal(props[1], results[0].StringProperty);
            }

            await DeleteAll<BasicModel>();
        }
    }
}
