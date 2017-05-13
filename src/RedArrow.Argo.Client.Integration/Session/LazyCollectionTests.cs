using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.TestUtils;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Client.Integration.Session
{
    public class LazyCollectionTests : IntegrationTest
    {
        public LazyCollectionTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper) :
            base(fixture, outputHelper)
        {
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task GetCollectionFromRelationship(Guid providerId, IEnumerable<Guid> patientIds)
        {
            // create the patients
            await Task.WhenAll(patientIds.Select(async x =>
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"{IntegrationTestFixture.Host}/data/");
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", IntegrationTestFixture.AccessToken.Value);
                    client.DefaultRequestHeaders.Add("api-version", "2");
                    client.DefaultRequestHeaders.Add("Titan-Data-Segmentation-Key",
                        "10000000-1000-0000-0000-000000000000");

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
                client.BaseAddress = new Uri($"{IntegrationTestFixture.Host}/data/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", IntegrationTestFixture.AccessToken.Value);
                client.DefaultRequestHeaders.Add("api-version", "2");
                client.DefaultRequestHeaders.Add("Titan-Data-Segmentation-Key", "10000000-1000-0000-0000-000000000000");

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

            using (var session = SessionFactory.CreateSession())
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

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task GetCollectionFromEmptyRelationship(Guid providerId)
        {
            // create the provider
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"{IntegrationTestFixture.Host}/data/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", IntegrationTestFixture.AccessToken.Value);
                client.DefaultRequestHeaders.Add("api-version", "2");
                client.DefaultRequestHeaders.Add("Titan-Data-Segmentation-Key", "10000000-1000-0000-0000-000000000000");

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
                            {"patients", new {data = Enumerable.Empty<Patient>()}}
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

            using (var session = SessionFactory.CreateSession())
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

        [Fact, Trait("Category", "Integration")]
        public async Task GetCollectionFromNullRelationship()
        {
            using (var session = SessionFactory.CreateSession())
            {
                var provider = await session.Create<Provider>();

                Assert.NotNull(provider.Patients);
                Assert.False(provider.Patients.Any());

                await session.Delete(provider);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task ForEachLoopOverCollection()
        {
            Guid providerId;
            ICollection<Guid> patientIds;
            using (var session = SessionFactory.CreateSession())
            {
                var provider = await session.Create<Provider>();
                providerId = provider.Id;

                var patientA = await session.Create<Patient>();
                var patientB = await session.Create<Patient>();
                var patientC = await session.Create<Patient>();

                patientIds = new List<Guid> {patientA.Id, patientB.Id, patientC.Id};

                provider.Patients.Add(patientA);
                provider.Patients.Add(patientB);
                provider.Patients.Add(patientC);

                await session.Update(provider);
            }

            using (var session = SessionFactory.CreateSession())
            {
                var provider = await session.Get<Provider>(providerId);

                // previously threw exception
                foreach (var patient in provider.Patients)
                {
                    Assert.NotNull(patient);
                    Assert.NotEqual(Guid.Empty, patient.Id);
                }
            }

            using (var session = SessionFactory.CreateSession())
            {
                var deleteProviderTask = session.Delete<Provider>(providerId);
                var deletePatientsTask = Task.WhenAll(patientIds.Select(x => session.Delete<Patient>(x)).ToArray());

                await Task.WhenAll(deleteProviderTask, deletePatientsTask);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task ClearRelated()
        {
            Guid providerId;
            IList<Guid> patientIds = new List<Guid>();

            using (var session = SessionFactory.CreateSession())
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

            using (var session = SessionFactory.CreateSession())
            {
                var provider = await session.Get<Provider>(providerId);

                Assert.NotNull(provider);
                Assert.NotNull(provider.Patients);
                Assert.All(patientIds, id => Assert.NotNull(provider.Patients.SingleOrDefault(x => x.Id == id)));

                Assert.Equal(3, provider.Patients.Count);
                provider.Patients.Clear();

                await session.Update(provider);
            }

            using (var session = SessionFactory.CreateSession())
            {
                var provider = await session.Get<Provider>(providerId);

                Assert.NotNull(provider);
                Assert.NotNull(provider.Patients);

                var patients = provider.Patients.ToArray();

                Assert.Equal(0, patients.Length);
            }

            using (var session = SessionFactory.CreateSession())
            {
                await session.Delete<Provider>(providerId);
                foreach (var patientId in patientIds)
                {
                    await session.Delete<Patient>(patientId);
                }
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task RemoveRelatedFetchedSeparately()
        {
            Guid providerId;
            IList<Guid> patientIds = new List<Guid>();

            using (var session = SessionFactory.CreateSession())
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

            using (var session = SessionFactory.CreateSession())
            {
                var provider = await session.Get<Provider>(providerId);

                Assert.NotNull(provider);
                Assert.NotNull(provider.Patients);
                Assert.All(patientIds, id => Assert.NotNull(provider.Patients.SingleOrDefault(x => x.Id == id)));

                var patients = provider.Patients.ToArray();

                Assert.Equal(3, patients.Length);
                var patient = patients[1];

                //get the patient separately
                var patientToRemove = await session.Get<Patient>(patient.Id);

                provider.Patients.Remove(patientToRemove);

                await session.Update(provider);
            }

            using (var session = SessionFactory.CreateSession())
            {
                var provider = await session.Get<Provider>(providerId);

                Assert.NotNull(provider);
                Assert.NotNull(provider.Patients);

                var patients = provider.Patients.ToArray();

                Assert.Equal(2, patients.Length);
                Assert.Equal(patientIds[0], patients[0].Id);
                Assert.Equal(patientIds[2], patients[1].Id);
            }

            using (var session = SessionFactory.CreateSession())
            {
                await session.Delete<Provider>(providerId);
                foreach (var patientId in patientIds)
                {
                    await session.Delete<Patient>(patientId);
                }
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task RemoveRelatedInCollection()
        {
            Guid providerId;
            IList<Guid> patientIds = new List<Guid>();

            using (var session = SessionFactory.CreateSession())
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

            using (var session = SessionFactory.CreateSession())
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

            using (var session = SessionFactory.CreateSession())
            {
                var provider = await session.Get<Provider>(providerId);

                Assert.NotNull(provider);
                Assert.NotNull(provider.Patients);

                var patients = provider.Patients.ToArray();

                Assert.Equal(2, patients.Length);
                Assert.Equal(patientIds[0], patients[0].Id);
                Assert.Equal(patientIds[2], patients[1].Id);
            }

            using (var session = SessionFactory.CreateSession())
            {
                await session.Delete<Provider>(providerId);
                foreach (var patientId in patientIds)
                {
                    await session.Delete<Patient>(patientId);
                }
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task AddRelated()
        {
            Guid providerId;
            Guid patientId;

            using (var session = SessionFactory.CreateSession())
            {
                var provider = await session.Create<Provider>();
                providerId = provider.Id;
                var patient = await session.Create<Patient>();
                patientId = patient.Id;

                provider.Patients.Add(patient);

                await session.Update(provider);
            }

            using (var session = SessionFactory.CreateSession())
            {
                var provider = await session.Get<Provider>(providerId);

                Assert.NotNull(provider);
                Assert.NotNull(provider.Patients);

                var patient = provider.Patients.FirstOrDefault();

                Assert.NotNull(patient);
                Assert.Equal(patientId, patient.Id);
            }

            using (var session = SessionFactory.CreateSession())
            {
                await session.Delete<Provider>(providerId);
                await session.Delete<Patient>(patientId);
            }
        }
    }
}