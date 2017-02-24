using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Config;
using RedArrow.Argo.Client.Session;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Integration
{
    public class PropertyBagTests : IClassFixture<IntegrationTestFixture>
    {
        private IntegrationTestFixture Fixture { get; }

        public PropertyBagTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper)
        {
            Fixture = fixture;
            Fixture.ConfigureLogging(outputHelper);
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task CreateModelWithPropertyBag
            (Guid expectedFoo, Guid expectedBar)
        {
            var sessionFactory = CreateSessionFactory();
            Guid patientId;
            using (var session = sessionFactory.CreateSession())
            {
                var patient = new Patient
                {
                    Unmapped = JObject.FromObject(new
                    {
                        foo = expectedFoo,
                        bar = expectedBar
                    })
                };

                patient = await session.Create(patient);

                patientId = patient.Id;
            }

            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(patientId);

                Assert.Equal(expectedFoo, patient.Unmapped["foo"].ToObject<Guid>());
                Assert.Equal(expectedBar, patient.Unmapped["bar"].ToObject<Guid>());
            }
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task UpdateModelWithPropertyBag
            (Guid expectedFoo, Guid expectedBar)
        {
            var sessionFactory = CreateSessionFactory();
            Guid patientId;

            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Create<Patient>();
                patientId = patient.Id;
            }

            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(patientId);
                patient.Unmapped = JObject.FromObject(new
                {
                    foo = expectedFoo,
                    bar = expectedBar
                });
                await session.Update(patient);
            }

            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(patientId);

                Assert.Equal(expectedFoo, patient.Unmapped["foo"].ToObject<Guid>());
                Assert.Equal(expectedBar, patient.Unmapped["bar"].ToObject<Guid>());
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
                    .Configure(scan => scan.AssemblyOf<Patient>())
                .BuildSessionFactory();
        }
    }
}
