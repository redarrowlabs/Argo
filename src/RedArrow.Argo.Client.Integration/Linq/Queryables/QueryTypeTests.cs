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

		[Theory, AutoData, Trait("Category", "Integration")]
		public async Task QueryByRelationship(Guid parentId, Guid[] childIds)
		{
			await DeleteAll<ComplexModel>();
			await DeleteAll<BasicModel>();

			using (var session = SessionFactory.CreateSession())
			{
				var parent = new ComplexModel
				{
					Id = parentId,
					BasicModels = childIds.Select(x => new BasicModel { Id = x })
				};

				await session.Create(parent);
			}

			using (var session = SessionFactory.CreateSession())
			{
				var result = session.CreateQuery<ComplexModel, BasicModel>(parentId, x => x.BasicModels).ToArray();
				Assert.NotNull(result);
				Assert.Equal(childIds.Length, result.Length);
				Assert.All(childIds, x => Assert.NotNull(result.SingleOrDefault(c => c.Id == x)));
			}

			await DeleteAll<ComplexModel>();
			await DeleteAll<BasicModel>();
		}
	}
}
