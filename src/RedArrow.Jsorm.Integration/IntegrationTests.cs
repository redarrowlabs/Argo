using System;
using System.Linq;
using System.Configuration;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Jsorm.Client.Collections.Generic;
using RedArrow.Jsorm.Client.Config;
using RedArrow.Jsorm.Client.Session;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Jsorm.Integration
{
    public class IntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private IntegrationTestFixture Fixture { get; }

        public IntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper)
        {
            Fixture = fixture;
            Fixture.ConfigureLogging(outputHelper);
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
        public async Task GetCollectionFromNullRelationship()
        {
            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.CreateSession())
            {
                var provider = await session.Create<Provider>();

                Assert.NotNull(provider.Patients);
                Assert.False(provider.Patients.Any());

                await session.Delete(provider);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task GetCollectionFromEmptyRelationship()
        {
            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.CreateSession())
            {
                // this is a "reserved" provider I have created specifically for this test
                var provider = await session.Get<Provider>(Guid.Parse("0b9709c5-492e-4f0f-ac46-546cbbde0b0b"));

                Assert.NotNull(provider.Patients);
                Assert.False(provider.Patients.Any());
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task GetCollectionFromRelationship()
        {
            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.CreateSession())
            {
                // this is a "reserved" provider I have created specifically for this test
                var provider = await session.Get<Provider>(Guid.Parse("e3f919b8-625b-42cf-8ef2-4c2489c51c5e"));

                Assert.NotNull(provider.Patients);
                Assert.True(provider.Patients.Any());
                Assert.Equal(8, provider.Patients.Count());
                Assert.All(provider.Patients, x =>
                {
                    Assert.Equal("Will", x.FirstName);
                    Assert.Equal("Power", x.LastName);
                });
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