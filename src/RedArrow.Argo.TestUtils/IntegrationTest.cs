using RedArrow.Argo.Client.Config;
using RedArrow.Argo.Client.Config.Pipeline;
using RedArrow.Argo.Client.Http.Handlers.ExceptionLogger;
using RedArrow.Argo.Client.Session;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Http.Handlers.Request;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.TestUtils
{
    public abstract class IntegrationTest : IClassFixture<IntegrationTestFixture>
    {
        protected IntegrationTestFixture Fixture { get; }
        protected ISessionFactory SessionFactory { get; }

        static IntegrationTest()
        {
            // Force the tests to allow TLS 1.2, since the test runners default to lower security protocols
            // Otherwise you get a SocketException when hitting sandbox.redarrow.io
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        protected IntegrationTest(IntegrationTestFixture fixture, ITestOutputHelper outputHelper)
        {
            Fixture = fixture;
            Fixture.ConfigureLogging(outputHelper);
            SessionFactory = CreateSessionFactory();
        }

        private ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure($"{IntegrationTestFixture.Host}/data/")
                .Remote()
                .Configure(httpClient =>
                {
                    httpClient
                            .DefaultRequestHeaders
                            .Authorization = new AuthenticationHeaderValue("Bearer", IntegrationTestFixture.AccessToken.Value);

                    httpClient.DefaultRequestHeaders.Add("Api-Version", "1.3");
                    httpClient.DefaultRequestHeaders.Add("Data-Segmentation-Key", "10000000-1000-0000-0000-000000000000");
                })
                .Configure(HttpClient)
                .Configure(HttpClientBuilder)
                .Use(new EtagRequestModifier())
                .Models()
                .Configure(scan => scan.AssemblyOf<Patient>())
                .BuildSessionFactory();
        }

        protected virtual Action<HttpClient> HttpClient => _ => { };
        protected virtual Action<IHttpClientBuilder> HttpClientBuilder => _ => { _.UseExceptionLogger(); };

        protected async Task DeleteAll<TModel>()
        {
            using (var session = SessionFactory.CreateSession())
            {
                var models = session.CreateQuery<TModel>().ToArray();

                await Task.WhenAll(models.Select(x => session.Delete(x)).ToArray());
            }
        }
    }
}