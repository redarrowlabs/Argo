using System;
using System.ComponentModel;
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

		[Fact, Trait("Category", "Integration")]
		public async Task CreateGetDeletePatient()
		{
			var initialFirstName = "Terry";
			var initialLastName = "Achey";

			var updatedLastName = "Bull";

            var sessionFactory = Fixture.Configuration
                .Models()
                .Configure(x => x.AddFromAssemblyOf<Patient>())
                .BuildSessionFactory();

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
            var sessionFactory = Fixture.Configuration
                .Models()
                .Configure(x => x.AddFromAssemblyOf<Patient>())
                .BuildSessionFactory();

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
            var sessionFactory = Fixture.Configuration
                .Models()
                .Configure(x => x.AddFromAssemblyOf<Patient>())
                .BuildSessionFactory();

            Guid crossSessionPatientId;

	        using (var session = sessionFactory.CreateSession())
	        {
	            var patient = new Patient
	            {
	                FirstName = "Marvin",
	                LastName = "Engen"
	            };

	            patient = await session.Create(patient);
	            crossSessionPatientId = patient.Id;
	        }

	        Guid crossSessionProviderId;

	        using (var session = sessionFactory.CreateSession())
	        {
	            var patient = await session.Get<Patient>(crossSessionPatientId);

	            patient.FirstName = "Marv";
	            patient.Provider = new Provider
	            {
	                FirstName = "Peggy",
	                LastName = "Engen"
	            };

	            await session.Update(patient);

	            crossSessionProviderId = patient.Provider.Id;
	        }

	        using (var session = sessionFactory.CreateSession())
	        {
	            var patient = await session.Get<Patient>(crossSessionPatientId);

                Assert.Equal(crossSessionProviderId, patient.Provider.Id);
	        }
        }
    }
}