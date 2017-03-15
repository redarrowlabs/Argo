using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.TestUtils;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Client.Integration.Linq.Queryables
{
    public class QuerySkipTests : IntegrationTest
    {
        public QuerySkipTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper) :
            base(fixture, outputHelper)
        {
        }

        [Theory, AutoData]
        public async Task Skip()
        {
            await DeleteAll<BasicModel>();

            using (var session = SessionFactory.CreateSession())
            {
                await Task.WhenAll(Enumerable.Range(0, 20).Select(i => session.Create<BasicModel>()).ToArray());
            }

            using (var session = SessionFactory.CreateSession())
            {
                var result = session.CreateQuery<BasicModel>().Skip(5).ToArray();

                Assert.NotNull(result);
                Assert.NotEmpty(result);

                Assert.Equal(15, result.Length);
            }

            await DeleteAll<BasicModel>();
        }
    }
}
