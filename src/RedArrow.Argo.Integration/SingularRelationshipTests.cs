using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Config;
using RedArrow.Argo.Client.Session;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Integration
{
    public class SingularRelationshipTests : IClassFixture<IntegrationTestFixture>
    {
        private IntegrationTestFixture Fixture { get; }

        public SingularRelationshipTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper)
        {
            Fixture = fixture;
            Fixture.ConfigureLogging(outputHelper);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task GetNullRelationship()
        {
            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Create<Patient>();

                var provider = patient.Provider;

                Assert.Null(provider);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task UpdateHasOneRelationWithSessionAttached()
        {
            var sessionFactory = CreateSessionFactory();

            Guid crossSessionPatientId;
            Guid crossSessionProviderId;
            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Create<Patient>();
                crossSessionPatientId = patient.Id;

                var provider = await session.Create<Provider>();
                crossSessionProviderId = provider.Id;

                patient.Provider = provider;

                await session.Update(patient);
            }

            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(crossSessionPatientId);

                var provider = patient.Provider;

                Assert.Equal(crossSessionPatientId, patient.Id);
                Assert.Equal(crossSessionProviderId, provider.Id);
            }

            using (var session = sessionFactory.CreateSession())
            {
                await session.Delete<Patient>(crossSessionPatientId);
                await session.Delete<Provider>(crossSessionProviderId);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task GetNullValueRelationship()
        {
            var sessionFactory = CreateSessionFactory();

            Guid patientId;
            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Create<Patient>();
                patientId = patient.Id;
                //explicityly set the provider to null to set up the NullValue JToken
                patient.Provider = null;
                await session.Update(patient);
            }

            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(patientId);
                Assert.Null(patient.Provider);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task GetNonNullRelationship()
        {
            var sessionFactory = CreateSessionFactory();

            Guid patientId;

            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Create<Patient>();
                patientId = patient.Id;

                var provider = patient.Provider;

                Assert.Null(provider);

                provider = await session.Create<Provider>();

                patient.Provider = provider;

                await session.Update(patient);
            }

            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(patientId);

                var provider = patient.Provider;

                Assert.NotNull(provider);

                //clean up
                await session.Delete(patient);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task SetNullReferenceToNull()
        {
            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Create<Patient>();

                Assert.NotNull(patient);
                Assert.Null(patient.Provider);

                // previously threw exception
                patient.Provider = null;

                await session.Delete(patient);
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
