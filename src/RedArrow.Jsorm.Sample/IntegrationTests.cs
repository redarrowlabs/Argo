using Ploeh.AutoFixture.Xunit2;
using RedArrow.Jsorm.Client.Config;
using RedArrow.Jsorm.Client.Session;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WovenByFody;
using Xunit;

namespace RedArrow.Jsorm.Sample
{
    public class IntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private IntegrationTestFixture Fixture { get; }

        public IntegrationTests(IntegrationTestFixture fixture)
        {
            Fixture = fixture;
        }

        [Theory, AutoData]
        public void GetSetAttributesTransient
            (string firstName, string lastName)
        {
            var patient = new Patient
            {
                FirstName = firstName,
                LastName = lastName
            };

            Assert.Equal(firstName, patient.FirstName);
            Assert.Equal(lastName, patient.LastName);
        }

        [Theory, Trait("Category", "Integration"), AutoData]
        public async Task GetSetAttributesPersisted
            (string firstName, string lastName)
        {
            var sessionFactory = CreateSessionFactory();

            Guid id;
            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Create<Patient>();
                id = patient.Id;

                patient.FirstName = firstName;
                patient.LastName = lastName;

                Assert.Equal(firstName, patient.FirstName);
                Assert.Equal(lastName, patient.LastName);
            }

            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(id);

                Assert.Null(patient.FirstName);
                Assert.Null(patient.LastName);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task CreateGetDeletePatient()
        {
            var initialFirstName = "Terry";
            var initialLastName = "Achey";

            var updatedLastName = "Bull";

            var sessionFactory = CreateSessionFactory();

            Guid crossSessionId;

            using (var session = sessionFactory.CreateSession())
            {
                var patient = new Patient
                {
                    FirstName = initialFirstName,
                    LastName = initialLastName
                };

                patient = await session.Create(patient);

                Assert.NotEqual(Guid.Empty, patient.Id);
                Assert.Equal(initialFirstName, patient.FirstName);
                Assert.Equal(initialLastName, patient.LastName);

                crossSessionId = patient.Id;

                var patientRef = await session.Get<Patient>(crossSessionId);

                Assert.Same(patient, patientRef);
            }
            // update!
            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(crossSessionId);

                Assert.Equal(crossSessionId, patient.Id);
                Assert.Equal(initialFirstName, patient.FirstName);
                Assert.Equal(initialLastName, patient.LastName);

                patient.LastName = updatedLastName;

                Assert.Equal(updatedLastName, patient.LastName);

                await session.Update(patient);
                Assert.Equal(initialFirstName, patient.FirstName);
                Assert.Equal(updatedLastName, patient.LastName);

                var patient2 = await session.Get<Patient>(crossSessionId);

                Assert.Same(patient, patient2);
            }
            // later that day...
            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(crossSessionId);

                Assert.Equal(crossSessionId, patient.Id);
                Assert.Equal(initialFirstName, patient.FirstName);
                Assert.Equal(updatedLastName, patient.LastName);
            }
            // cleanup
            using (var session = sessionFactory.CreateSession())
            {
                await session.Delete<Patient>(crossSessionId);

                var patient = await session.Get<Patient>(crossSessionId);

                Assert.Null(patient);
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
        public async Task UpdateModelWithTransientReference()
        {
            Guid crossSessionPatientId;

            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.CreateSession())
            {
                var patient = new Patient
                {
                    FirstName = "Joe",
                    LastName = "King"
                };

                patient = await session.Create(patient);
                crossSessionPatientId = patient.Id;
            }

            Guid crossSessionProviderId;

            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(crossSessionPatientId);

                patient.FirstName = "Joseph";
                patient.Provider = new Provider
                {
                    FirstName = "Kerry",
                    LastName = "Oki"
                };

                await session.Update(patient);

                crossSessionProviderId = patient.Provider.Id;
            }

            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(crossSessionPatientId);

                Assert.Equal(crossSessionProviderId, patient.Provider.Id);
            }

            // cleanup
            using (var session = sessionFactory.CreateSession())
            {
                await session.Delete<Patient>(crossSessionPatientId);
                await session.Delete<Provider>(crossSessionProviderId);
            }
        }

        private ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure($"{Fixture.Host}/data/")
                .Remote()
                    .Configure(httpClient => httpClient
                        .DefaultRequestHeaders
                        .Authorization = new AuthenticationHeaderValue("Bearer", Fixture.AccessToken))
                .Models()
                    .Configure(scan => scan.AssemblyOf<Patient>())
                .BuildSessionFactory();
        }
    }
}