using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Config;
using RedArrow.Argo.Client.Session;
using RedArrow.Compass.CareTeam.CaseManagement.Model;
using Xunit;
using Xunit.Abstractions;

namespace Compass.Integration
{
    public class CompassCaseDefinitionTests : IClassFixture<IntegrationTestFixture>
    {
        private IntegrationTestFixture Fixture { get; }

        public CompassCaseDefinitionTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper)
        {
            Fixture = fixture;
            Fixture.ConfigureLogging(outputHelper);
        }

        [Fact]
        public async Task Create()
        {
            var sessionFactory = CreateSessionFactory();

            var model = CompassCaseDefinition.NewCaseInstance();

            Guid modelId;

            using (var session = sessionFactory.CreateSession())
            {
                model = await session.Create(model);
                modelId = model.Id;
            }

            using (var session = sessionFactory.CreateSession())
            {
                var fetched = await session.Get<CompassCaseDefinition>(modelId);
            }
        }

        private ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure($"{Fixture.Host}/data/")
                .Remote()
                    .Configure(httpClient =>
                    {
                        httpClient
                            .DefaultRequestHeaders
                            .Authorization = new AuthenticationHeaderValue("Bearer", Fixture.AccessToken);

                        httpClient.DefaultRequestHeaders.Add("Api-Version", "2");
                        httpClient.DefaultRequestHeaders.Add("Titan-Data-Segmentation-Key", "10000000-1000-0000-0000-000000000000");
                    })
                .Models()
                    .Configure(scan => scan.AssemblyOf<CompassCaseDefinition>())
                .BuildSessionFactory();
        }
    }
}
