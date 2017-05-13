using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Config;
using RedArrow.Argo.Client.Config.Pipeline;
using RedArrow.Argo.Client.Session;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.TestUtils
{
    public abstract class IntegrationTest : IClassFixture<IntegrationTestFixture>
    {
        protected IntegrationTestFixture Fixture { get; }
        protected ISessionFactory SessionFactory { get; }

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

                    httpClient.DefaultRequestHeaders.Add("Api-Version", "1");
                    httpClient.DefaultRequestHeaders.Add("Data-Segmentation-Key", "10000000-1000-0000-0000-000000000000");
                })
                .Configure(HttpClient)
                .Configure(HttpClientBuilder)
                .Models()
                .Configure(scan => scan.AssemblyOf<Patient>())
                .BuildSessionFactory();
        }

        protected virtual Action<HttpClient> HttpClient => _ => { };
        protected virtual Action<IHttpClientBuilder> HttpClientBuilder => _ => { };

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