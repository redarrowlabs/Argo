using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Config;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Session;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Integration
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

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task GetCollectionFromEmptyRelationship(Guid providerId)
        {
            // create the provider
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://titan-test.centralus.cloudapp.azure.com/api/data/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Fixture.AccessToken);

                var body = new
                {
                    data = new
                    {
                        id = providerId,
                        type = "integration-test-provider",
                        attributes = new
                        {
                            firstName = "Dr.",
                            lastName = "Payne"
                        },
                        relationships = new Dictionary<string, dynamic>
                        {
                            {"patients", new { data = Enumerable.Empty<Patient>()}}
                        }
                    }
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(body),
                    Encoding.UTF8,
                    "application/vnd.api+json");
                var providerResponse = await client.PostAsync("integration-test-provider", content);

                Assert.True(providerResponse.IsSuccessStatusCode);
            }

            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.CreateSession())
            {
                // this is a "reserved" provider I have created specifically for this test
                var provider = await session.Get<Provider>(providerId);

                Assert.NotNull(provider);
                Assert.Equal("Dr.", provider.FirstName);
                Assert.Equal("Payne", provider.LastName);
                Assert.NotNull(provider.Patients);
                Assert.False(provider.Patients.Any());

                await session.Delete<Provider>(providerId);
            }
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task GetCollectionFromRelationship(Guid providerId, IEnumerable<Guid> patientIds)
        {
            // create the patients
            await Task.WhenAll(patientIds.Select(async x =>
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://titan-test.centralus.cloudapp.azure.com/api/data/");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Fixture.AccessToken);

                    var body = new
                    {
                        data = new
                        {
                            id = x,
                            type = "integration-test-patient",
                            attributes = new
                            {
                                firstName = "Will",
                                lastName = "Power"
                            }
                        }
                    };

                    var content = new StringContent(
                        JsonConvert.SerializeObject(body),
                        Encoding.UTF8,
                        "application/vnd.api+json");
                    var response = await client.PostAsync("integration-test-patient", content);
                    response.EnsureSuccessStatusCode();
                }
            }));

            // create the provider
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://titan-test.centralus.cloudapp.azure.com/api/data/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Fixture.AccessToken);

                var body = new
                {
                    data = new
                    {
                        id = providerId,
                        type = "integration-test-provider",
                        attributes = new
                        {
                            firstName = "Dr.",
                            lastName = "Payne"
                        },
                        relationships = new Dictionary<string, dynamic>
                        {
                            {
                                "patients",
                                new
                                {
                                    data = patientIds.Select(x => new {id = x, type = "integration-test-patient"})
                                }
                            }
                        }
                    }
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(body),
                    Encoding.UTF8,
                    "application/vnd.api+json");
                var response = await client.PostAsync("integration-test-provider", content);
                response.EnsureSuccessStatusCode();
            }

            var sessionFactory = CreateSessionFactory();

            using (var session = sessionFactory.CreateSession())
            {
                // this is a "reserved" provider I have created specifically for this test
                var provider = await session.Get<Provider>(providerId);

                Assert.NotNull(provider.Patients);
                Assert.True(provider.Patients.Any());
                Assert.Equal(patientIds.Count(), provider.Patients.Count());
                Assert.All(provider.Patients, x =>
                {
                    Assert.Equal("Will", x.FirstName);
                    Assert.Equal("Power", x.LastName);
                });

                await session.Delete<Provider>(providerId);
                await Task.WhenAll(patientIds.Select(x => session.Delete<Patient>(x)));
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task AddRelated()
        {
            var sessionFactory = CreateSessionFactory();

            Guid providerId;
            Guid patientId;

            using (var session = sessionFactory.CreateSession())
            {
                var provider = await session.Create<Provider>();
                providerId = provider.Id;
                var patient = await session.Create<Patient>();
                patientId = patient.Id;

                provider.Patients.Add(patient);

                await session.Update(provider);
            }

            using (var session = sessionFactory.CreateSession())
            {
                var provider = await session.Get<Provider>(providerId);

                Assert.NotNull(provider);
                Assert.NotNull(provider.Patients);

                var patient = provider.Patients.First();

                Assert.NotNull(patient);
                Assert.Equal(patientId, patient.Id);
            }

            using (var session = sessionFactory.CreateSession())
            {
                await session.Delete<Provider>(providerId);
                await session.Delete<Patient>(patientId);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task RemoveRelated()
        {
            var sessionFactory = CreateSessionFactory();

            Guid providerId;
            IList<Guid> patientIds = new List<Guid>();

            using (var session = sessionFactory.CreateSession())
            {
                var provider = await session.Create<Provider>();
                providerId = provider.Id;

                for (var i = 0; i < 3; i++)
                {
                    var patient = await session.Create<Patient>();
                    patientIds.Add(patient.Id);
                    provider.Patients.Add(patient);
                }

                await session.Update(provider);
            }

            using (var session = sessionFactory.CreateSession())
            {
                var provider = await session.Get<Provider>(providerId);

                Assert.NotNull(provider);
                Assert.NotNull(provider.Patients);
                Assert.All(patientIds, id => Assert.NotNull(provider.Patients.SingleOrDefault(x => x.Id == id)));

                var patients = provider.Patients.ToArray();

                Assert.Equal(3, patients.Length);
                var removePatient = patients[1];
                provider.Patients.Remove(removePatient);

                await session.Update(provider);
            }

            using (var session = sessionFactory.CreateSession())
            {
                var provider = await session.Get<Provider>(providerId);

                Assert.NotNull(provider);
                Assert.NotNull(provider.Patients);

                // TODO: this is not initializing the collection
                // TODO: because Count returns 0 (I think?)
                var patients = provider.Patients.ToArray();

                Assert.Equal(2, patients.Length);
                Assert.Equal(patientIds[0], patients[0].Id);
                Assert.Equal(patientIds[2], patients[1].Id);
            }

            using (var session = sessionFactory.CreateSession())
            {
                await session.Delete<Provider>(providerId);
                foreach (var patientId in patientIds)
                {
                    await session.Delete<Patient>(patientId);
                }
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