using System;
using System.Threading.Tasks;
using RedArrow.Argo.Client.Config.Pipeline;
using RedArrow.Argo.Client.Http.Handlers.GZip;
using RedArrow.Argo.Client.Http.Handlers.Response;
using RedArrow.Argo.TestUtils;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Client.Integration.Http.Handlers.GZip
{
    public class GZipCompressionHandlerTests : IntegrationTest
    {
        protected override Action<IHttpClientBuilder> HttpClientBuilder => builder => builder
            .UseResponseHandler(new ResponseHandlerOptions
            {
                ResponseReceived = response =>
                {
                    Assert.Equal("gzip", response.RequestMessage.Content.Headers.ContentEncoding.ToString());
                    Assert.Equal("gzip", response.Content.Headers.ContentEncoding.ToString());

                    return Task.CompletedTask;
                }
            })
            .UseGZipCompression();

        public GZipCompressionHandlerTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper) :
            base(fixture, outputHelper)
        {
        }

        [Fact, Trait("Category", "Integration")]
        public async Task CreateGetDeletePatient()
        {
            var initialFirstName = "Terry";
            var initialLastName = "Achey";

            var updatedLastName = "Bull";

            Guid crossSessionId;

            using (var session = SessionFactory.CreateSession())
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
            using (var session = SessionFactory.CreateSession())
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
            using (var session = SessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(crossSessionId);

                Assert.Equal(crossSessionId, patient.Id);
                Assert.Equal(initialFirstName, patient.FirstName);
                Assert.Equal(updatedLastName, patient.LastName);
            }
            // cleanup
            using (var session = SessionFactory.CreateSession())
            {
                await session.Delete<Patient>(crossSessionId);
            }
        }

    }
}
