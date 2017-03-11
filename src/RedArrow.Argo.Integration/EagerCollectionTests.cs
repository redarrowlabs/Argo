using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedArrow.Argo.TestUtils;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Integration
{
    public class EagerCollectionTests : IntegrationTest
    {
        public EagerCollectionTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper) :
            base(fixture, outputHelper)
        {
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

        [Fact]
        public async Task GetModelWithEagerLoadedCollection()
        {
            var sessionFactory = CreateSessionFactory();

            Guid id;
            using (var session = sessionFactory.CreateSession())
            {
                var provider = new Provider
                {
                    FirstName = "Bill",
                    LastName = "Nye",
                    Patients = new List<Patient>
                    {
                        new Patient(),
                        new Patient(),
                        new Patient()
                    }
                };

                provider = await session.Create(provider);
                id = provider.Id;
            }

            using (var session = sessionFactory.CreateSession())
            {
                var provider = await session.Get<Provider>(id);

                Assert.Equal(3, provider.Patients.Count);

                foreach (var patient in provider.Patients)
                {
                    await session.Delete(patient);
                }
                await session.Delete(provider);
            }
        }
    }
}
