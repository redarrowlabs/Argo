using AssemblyToWeave;
using RedArrow.Jsorm.Config;
using System;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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

        [Fact, Category("Integration")]
        public async Task CreateGetDeletePatient()
        {
            var firstName = "Terry";
            var lastName = "Achey";

            var sessionFactory = Fluently.Configure()
                .Remote()
                    .Configure(x => x.BaseAddress = new Uri("http://localhost:8082/data/api/"))
                    .Configure(x => x.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Fixture.AccessToken))
                .Models()
                    .Configure(x => x.AddFromAssemblyOf<Patient>())
                .BuildSessionFactory();

            Guid crossSessionId;

            using (var session = sessionFactory.CreateSession())
            {
                var patient = new Patient
                {
                    FirstName = firstName,
                    LastName = lastName
                };

                patient = await session.Create(patient);

                Assert.NotEqual(Guid.Empty, patient.Id);
                Assert.Equal(firstName, patient.FirstName);
                Assert.Equal(lastName, patient.LastName);

                crossSessionId = patient.Id;

                var patientRef = await session.Get<Patient>(crossSessionId);

                Assert.Same(patient, patientRef);
            }
            // dispose, clear state
            using (var session = sessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(crossSessionId);

                Assert.Equal(crossSessionId, patient.Id);
                Assert.Equal(firstName, patient.FirstName);
                Assert.Equal(lastName, patient.LastName);
            }
            // dispose, clear state
            using (var session = sessionFactory.CreateSession())
            {
                await session.Delete<Patient>(crossSessionId);

                var patient = await session.Get<Patient>(crossSessionId);

                Assert.Null(patient);
            }
        }
    }
}