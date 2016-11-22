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

		[Fact, Trait("Category", "Integration")]
		public async Task CreateGetDeletePatient()
		{
			var initialFirstName = "Terry";
			var initialLastName = "Achey";

			var updatedLastName = "Bull";

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
			var sessionFactory = Fluently.Configure()
				.Remote()
				.Configure(x => x.BaseAddress = new Uri("http://localhost:8082/data/api/"))
				.Configure(x => x.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Fixture.AccessToken))
				.Models()
				.Configure(x => x.AddFromAssemblyOf<Patient>())
				.BuildSessionFactory();

			using (var session = sessionFactory.CreateSession())
			{
			}
		}
	}
}