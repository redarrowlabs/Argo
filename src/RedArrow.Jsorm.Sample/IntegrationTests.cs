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

		[Fact(Skip = "need to combine read/write resource endpoints first"), Category("Integration")]
		public async Task CreateGetDeletePatient()
		{
			var firstName = "Terry";
			var lastName = "Achey";

			var sessionFactory = Fluently.Configure()
				.Remote()
					.Configure(x => x.BaseAddress = new Uri("http://titan-test.centralus.cloudapp.azure.com/resources/write/api/"))
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
				Assert.Equal(lastName, lastName);

				crossSessionId = patient.Id;
			}
			// dispose, clear state
			using (var session = sessionFactory.CreateSession())
			{
				var patient = await session.Get<Patient>(crossSessionId);

				Assert.Equal(crossSessionId, patient.Id);
				Assert.Equal(firstName, patient.FirstName);
				Assert.Equal(lastName, lastName);
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