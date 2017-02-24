using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Config;
using RedArrow.Argo.Client.Session;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Integration
{
    public class EagerCollectionTests : IClassFixture<IntegrationTestFixture>
    {
        private IntegrationTestFixture Fixture { get; }

        public EagerCollectionTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper)
        {
            Fixture = fixture;
            Fixture.ConfigureLogging(outputHelper);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task AddIncluded()
        {
            var sessionFactory = CreateSessionFactory();

            Guid providerId;
            Guid patientId;
            Guid companyId;

            var stubInsuranceCompany = new Company
            {
                Name = "Primo Insurance Brah!",
                Address = "123 Main St",
                PhoneNumber = "800-555-1234",
            };

            var stubPatient1 = new Patient
            {
                FirstName = "Deltron",
                LastName = "3030",
                Age = 45,
                AccountBalance = (decimal)15.99,
                Insurance = stubInsuranceCompany
            };

            var stubPatient2 = new Patient
            {
                FirstName = "Del",
                LastName = "The Funky Homosapien",
                Age = 35,
                AccountBalance = (decimal)125.99,
                Insurance = stubInsuranceCompany
            };

            var stubProvider = new Provider
            {
                FirstName = "Bob",
                LastName = "Dobblena",
                Patients = new List<Patient> { stubPatient1, stubPatient2 }
            };

            using (var session = sessionFactory.CreateSession())
            {
                var provider = await session.Create(stubProvider);
                providerId = provider.Id;
                patientId = provider.Patients.First().Id;
            }

            using (var session = sessionFactory.CreateSession())
            {
                var provider = await session.Get<Provider>(providerId);

                Assert.NotNull(provider);
                Assert.NotNull(provider.Patients);

                var patient = provider.Patients.First();
                companyId = patient.Insurance.Id;

                Assert.NotNull(patient);
                Assert.Equal(patientId, patient.Id);
            }

            using (var session = sessionFactory.CreateSession())
            {
                await session.Delete<Provider>(providerId);
                await session.Delete<Patient>(patientId);
                await session.Delete<Company>(companyId);
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

                        httpClient.DefaultRequestHeaders.Add("api-version", "2");
                        httpClient.DefaultRequestHeaders.Add("Titan-Data-Segmentation-Key", "10000000-1000-0000-0000-000000000000");
                    })
                .Models()
                    .Configure(scan => scan.AssemblyOf<Patient>())
                .BuildSessionFactory();
        }
    }
}
