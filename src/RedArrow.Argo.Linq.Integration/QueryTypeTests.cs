using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Linq.Extensions;
using RedArrow.Argo.TestUtils;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Linq.Integration
{
    public class QueryTypeTests : IntegrationTest
    {
        public QueryTypeTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper) :
            base(fixture, outputHelper)
        {
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task QueryByType(Guid[] ids)
		{
			await DeleteAll<BasicModel>();

			using (var session = SessionFactory.CreateSession())
			{
				await Task.WhenAll(ids.Select(id => session.Create(new BasicModel { Id = id })).ToArray());
			}

            using (var session = SessionFactory.CreateSession())
            {
                var results = session.CreateQuery<BasicModel>().ToArray();

                Assert.NotNull(results);
                Assert.Equal(ids.Length, results.Length);
                Assert.All(results, result =>
                {
                    Assert.Contains(result.Id, ids);
                });
			}

			await DeleteAll<BasicModel>();
		}
    }
}
