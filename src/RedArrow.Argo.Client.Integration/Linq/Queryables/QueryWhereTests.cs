using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.TestUtils;
using System;
using System.Linq;
using System.Threading.Tasks;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Client.Integration.Linq.Queryables
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
                    .Select((x, i) => session.Create(new AllPropertyTypes { StringProperty = props[i] }))
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

            await DeleteAll<AllPropertyTypes>();
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task Where__When_WhereStringEquals__Then_ReturnMatchingResults
            (string[] props)
        {
            await DeleteAll<AllPropertyTypes>();

            using (var session = SessionFactory.CreateSession())
            {
                await Task.WhenAll(props
                    .Select((x, i) => session.Create(new AllPropertyTypes { StringProperty = props[i] }))
                    .ToArray());
            }

            using (var session = SessionFactory.CreateSession())
            {
                var expectedProp = props[1];
                var results = session.CreateQuery<AllPropertyTypes>()
                    .Where(x => x.StringProperty.Equals(expectedProp))
                    .ToArray();

                Assert.Equal(1, results.Length);
                Assert.Equal(props[1], results[0].StringProperty);
            }

            await DeleteAll<AllPropertyTypes>();
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task Where__When_WhereStringContains__Then_ReturnMatchingResults
            (string[] props)
        {
            await DeleteAll<AllPropertyTypes>();

            using (var session = SessionFactory.CreateSession())
            {
                await Task.WhenAll(props
                    .Select((x, i) => session.Create(new AllPropertyTypes { StringProperty = props[i] }))
                    .ToArray());
            }

            using (var session = SessionFactory.CreateSession())
            {
                var expectedProp = props[1];
                var results = session.CreateQuery<AllPropertyTypes>()
                    .Where(x => x.StringProperty.Contains(expectedProp))
                    .ToArray();

                Assert.Equal(1, results.Length);
                Assert.Equal(props[1], results[0].StringProperty);
            }

            await DeleteAll<AllPropertyTypes>();
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task Where__When_WhereStringStartsWith__Then_ReturnMatchingResults
            (string[] props)
        {
            await DeleteAll<AllPropertyTypes>();

            using (var session = SessionFactory.CreateSession())
            {
                await Task.WhenAll(props
                    .Select((x, i) => session.Create(new AllPropertyTypes { StringProperty = props[i] }))
                    .ToArray());
            }

            using (var session = SessionFactory.CreateSession())
            {
                var expectedProp = props[1];
                var results = session.CreateQuery<AllPropertyTypes>()
                    .Where(x => x.StringProperty.StartsWith(expectedProp))
                    .ToArray();

                Assert.Equal(1, results.Length);
                Assert.Equal(props[1], results[0].StringProperty);
            }

            await DeleteAll<AllPropertyTypes>();
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task Where__When_WhereStringEndsWith__Then_ReturnMatchingResults
            (string[] props)
        {
            await DeleteAll<AllPropertyTypes>();

            using (var session = SessionFactory.CreateSession())
            {
                await Task.WhenAll(props
                    .Select((x, i) => session.Create(new AllPropertyTypes { StringProperty = props[i] }))
                    .ToArray());
            }

            using (var session = SessionFactory.CreateSession())
            {
                var expectedProp = props[1];
                var results = session.CreateQuery<AllPropertyTypes>()
                    .Where(x => x.StringProperty.EndsWith(expectedProp))
                    .ToArray();

                Assert.Equal(1, results.Length);
                Assert.Equal(props[1], results[0].StringProperty);
            }

            await DeleteAll<AllPropertyTypes>();
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task Where__When_WhereHasMetaClauses__Then_ReturnMatchingResults
            (string[] meta, string[] attribute)
        {
            await DeleteAll<Widget>();

            var now = DateTime.UtcNow;
            using (var session = SessionFactory.CreateSession())
            {
                await Task.WhenAll(meta
                    .Select((x, i) => session.Create(
                        new Widget { Name = attribute[i], Whatever = meta[i] }
                    ))
                    .ToArray());
            }

            using (var session = SessionFactory.CreateSession())
            {
                var expectedMeta = meta[1];
                var expectedAttr = attribute[1];
                var results = session.CreateQuery<Widget>()
                    .Where(x => x.Name == expectedAttr && x.Whatever == expectedMeta && x.CreatedAt >= now)
                    .ToArray();

                Assert.Equal(1, results.Length);
                Assert.Equal(attribute[1], results[0].Name);
                Assert.Equal(meta[1], results[0].Whatever);
                Assert.NotSame(DateTime.MinValue, results[0].CreatedAt);
            }

            await DeleteAll<Widget>();
        }
    }
}