using System;
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
	public class QuerySingleTests : IntegrationTest
	{
		public QuerySingleTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper) :
			base(fixture, outputHelper)
		{
		}

		[Theory, AutoData, Trait("Category", "Integration")]
		public async Task QueryFirst__When_Results__Then_ReturnFirst(Guid[] ids)
		{
			// delete any pre-existing garbage
			await DeleteAll<BasicModel>();

			using (var session = SessionFactory.CreateSession())
			{
				await Task.WhenAll(ids.Select((t, i) => session.Create(new BasicModel
				{
					Id = t
				})).ToArray());
			}

			using (var session = SessionFactory.CreateSession())
			{
				var result = session.CreateQuery<BasicModel>().First();

				Assert.NotNull(result);

				await DeleteAll<BasicModel>();
			}
		}
	}
}
