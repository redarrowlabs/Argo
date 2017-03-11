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
            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.CreateSession())
            {
                await Task.WhenAll(ids.Select(id => session.Create(new BasicModel { Id = id })).ToArray());
            }

            using (var session = sessionFactory.CreateSession())
            {
                var results = session.CreateQuery<BasicModel>().ToArray();

                Assert.NotNull(results);
                Assert.Equal(ids.Length, results.Length);
                Assert.All(results, result =>
                {
                    Assert.Contains(result.Id, ids);
                });
            }

            using (var session = sessionFactory.CreateSession())
            {
                await Task.WhenAll(ids.Select(id => session.Delete<BasicModel>(id)).ToArray());
            }
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task OrderBySimple(Guid[] ids, string[] props)
        {
            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.CreateSession())
            {
                await Task.WhenAll(ids.Select((t, i) => session.Create(new BasicModel
                {
                    Id = t,
                    PropA = props[i]
                })).ToArray());
            }

            using (var session = sessionFactory.CreateSession())
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

                await Task.WhenAll(results.Select(result => session.Delete(result)).ToArray());
            }
        }
    }
}
