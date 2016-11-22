using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AssemblyToWeave;
using RedArrow.Jsorm.Config;
using Xunit;

namespace RedArrow.Jsorm.Tests.Session
{
	public class SessionTests
	{
		[Fact]
		public async Task Create__Given_NoArgs__Then_PostAndCacheNewModel()
		{
			var basePath = "http://www.test.com/";

			var resourceType = "integration-test-patient";
			var expectedPatientId = Guid.NewGuid();
			
			var fakeHandler = new FakeResponseHandler();
			fakeHandler.AddFakeResponse(
				new Uri($"{basePath}{resourceType}"),
				new HttpResponseMessage(HttpStatusCode.Accepted)
				{
					Headers = { Location = new Uri($"{resourceType}/{expectedPatientId}", UriKind.Relative)}
				});

			var sessionFactory = Fluently.Configure()
				.Remote()
					.Create(() => new HttpClient(fakeHandler))
					.Configure(x => x.BaseAddress = new Uri(basePath))
				.Models()
					.Configure(x => x.AddFromAssemblyOf<Patient>())
				.BuildSessionFactory();

			var session = sessionFactory.CreateSession();
			var result = await session.Create<Patient>();

			Assert.NotNull(result);
			Assert.IsType<Patient>(result);
			
			Assert.NotEqual(Guid.Empty, result.Id);
		}

		[Fact]
		public async Task Create__When_Disposed__Then_ThrowEx()
		{
			var session = Fluently.Configure()
				.BuildSessionFactory()
				.CreateSession();

			session.Dispose();

			await Assert.ThrowsAsync<Exception>(() => session.Create<Patient>());
		}

		[Fact]
		public async Task ModelId__Given_Model__When_ModelTypeRegistered__Then_ReturnModelId()
		{
			var basePath = "http://www.test.com/";

			var resourceType = "integration-test-patient";
			var expectedPatientId = Guid.NewGuid();

			var fakeHandler = new FakeResponseHandler();
			fakeHandler.AddFakeResponse(
				new Uri($"{basePath}{resourceType}"),
				new HttpResponseMessage(HttpStatusCode.Accepted)
				{
					Headers = { Location = new Uri($"{resourceType}/{expectedPatientId}", UriKind.Relative) }
				});

			var session = Fluently.Configure()
				.Remote()
					.Create(() => new HttpClient(fakeHandler))
					.Configure(x => x.BaseAddress = new Uri(basePath))
				.Models()
					.Configure(x => x.AddFromAssemblyOf<Patient>())
				.BuildSessionFactory()
				.CreateSession();

			var model = await session.Create<Patient>();
			var modelId = model.Id;

			var result = ((Jsorm.Session.Session)session).ModelId(model);

			Assert.Equal(modelId, result);
		}

		[Fact]
		public void ModelId__Given_Model__When_ModelTypeNotRegistered__Then_ThrowEx()
		{
			var session = Fluently.Configure()
				.BuildSessionFactory()
				.CreateSession();

			var model = new Patient();

			Assert.Throws<Exception>(() => ((Jsorm.Session.Session) session).ModelId(model));
		}

		[Fact]
		public void Dispose__Given_ConfiguredHttpClient__When_Disposed__Then_DisposeClient()
		{
			var fakeHandler = new FakeResponseHandler();

			var sessionFactory = Fluently.Configure()
				.Remote()
					.Create(() => new HttpClient(fakeHandler, true))
				.BuildSessionFactory();

			var session = sessionFactory.CreateSession();
			session.Dispose();


			Assert.True(((Jsorm.Session.Session)session).Disposed);
			Assert.True(fakeHandler.Disposed);
		}
	}
}
