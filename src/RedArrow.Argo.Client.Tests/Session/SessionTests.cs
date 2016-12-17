using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Config;
using RedArrow.Argo.TestUtils.XUnitSink;
using Serilog;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Client.Tests.Session
{
    public class SessionTests
    {
        public SessionTests(ITestOutputHelper outputHelper)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.XunitTestOutput(outputHelper)
                .CreateLogger();
        }

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
                    Headers = { Location = new Uri($"{resourceType}/{expectedPatientId}", UriKind.Relative) }
                });

            var sessionFactory = Fluently.Configure(basePath)
                .Remote()
                    .Create(() => new HttpClient(fakeHandler))
                    .Configure(x => x.BaseAddress = new Uri(basePath))
                .Models()
                    .Configure(x => x.AssemblyOf<Patient>())
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
            var session = Fluently.Configure("http://mock.test.com")
                .BuildSessionFactory()
                .CreateSession();

            session.Dispose();

            await Assert.ThrowsAsync<Exception>(() => session.Create<Patient>());
        }

        [Fact]
        public void Dispose__Given_ConfiguredHttpClient__When_Disposed__Then_DisposeClient()
        {
            var fakeHandler = new FakeResponseHandler();

            var sessionFactory = Fluently.Configure("http://mock.test.com")
                .Remote()
                    .Create(() => new HttpClient(fakeHandler, true))
                .BuildSessionFactory();

            var session = sessionFactory.CreateSession();
            session.Dispose();

            Assert.True(((Client.Session.Session) session).Disposed);
            Assert.True(fakeHandler.Disposed);
        }
    }
}