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
	public class QueryOrderByTests : IntegrationTest
	{
		public QueryOrderByTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper) :
			base(fixture, outputHelper)
		{
		}

		[Theory, AutoData, Trait("Category", "Integration")]
		public async Task OrderBySimple(Guid[] ids, string[] props)
		{
			// delete any pre-existing garbage
			await DeleteAll<BasicModel>();

			using (var session = SessionFactory.CreateSession())
			{
				await Task.WhenAll(ids.Select((t, i) => session.Create(new BasicModel
				{
					Id = t,
					PropA = props[i]
				})).ToArray());
			}

			using (var session = SessionFactory.CreateSession())
			{
				var results = session.CreateQuery<BasicModel>()
					.OrderBy(x => x.PropA)
					.ToArray();

				Assert.NotNull(results);
				Assert.Equal(ids.Length, results.Length);
				Assert.All(results, result =>
				{
					Assert.Contains(result.Id, ids);
				});

				var orderedPropAs = props.OrderBy(x => x).ToArray();
				for(var i = 0; i < ids.Length; ++i)
				{
					Assert.Equal(orderedPropAs[i], results[i].PropA);
				}

				// cleanup
				await DeleteAll<BasicModel>();
			}
		}

		[Theory, AutoData, Trait("Category", "Integration")]
		public async Task OrderByDsecendingSimple(Guid[] ids, string[] props)
		{
			// delete any pre-existing garbage
			await DeleteAll<BasicModel>();

			using (var session = SessionFactory.CreateSession())
			{
				await Task.WhenAll(ids.Select((t, i) => session.Create(new BasicModel
				{
					Id = t,
					PropA = props[i]
				})).ToArray());
			}

			using (var session = SessionFactory.CreateSession())
			{
				var results = session.CreateQuery<BasicModel>()
					.OrderByDescending(x => x.PropA)
					.ToArray();

				Assert.NotNull(results);
				Assert.Equal(ids.Length, results.Length);
				Assert.All(results, result =>
				{
					Assert.Contains(result.Id, ids);
				});

				var orderedPropAs = props.OrderByDescending(x => x).ToArray();
				for (var i = 0; i < ids.Length; ++i)
				{
					Assert.Equal(orderedPropAs[i], results[i].PropA);
				}

				// cleanup
				await DeleteAll<BasicModel>();
			}
		}
	}
}
