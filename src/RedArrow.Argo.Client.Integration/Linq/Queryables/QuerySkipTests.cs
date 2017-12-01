using System;
using System.Linq;
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

        [Fact, Trait("Category", "Integration")]
        public async Task Skip()
        {
            await DeleteAll<BasicModel>();

            var models = Enumerable.Range(0, 20)
                .Select(i => new BasicModel
                {
                    Id = Guid.NewGuid(),
                    PropA = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                })
                .ToArray();

            using (var session = SessionFactory.CreateSession())
            {
                await Task.WhenAll(models.Select(x => session.Create(x)).ToArray());
            }

            var expectedModels = models
                .OrderBy(x => x.PropA, StringComparer.Ordinal)
                .Skip(5)
                .ToArray();

            using (var session = SessionFactory.CreateSession())
            {
                var result = session.CreateQuery<BasicModel>()
                    .OrderBy(x => x.PropA)
                    .Skip(5)
                    .ToArray();

                Assert.NotNull(result);
                Assert.NotEmpty(result);
                Assert.Equal(expectedModels.Length, result.Length);
                for (var i = 0; i < result.Length; i++)
                {
                    Assert.Equal(expectedModels[i].Id, result[i].Id);
                }
            }

            await DeleteAll<BasicModel>();
        }
    }
}