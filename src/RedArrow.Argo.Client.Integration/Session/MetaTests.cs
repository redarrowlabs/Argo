using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.TestUtils;
using System;
using System.Threading.Tasks;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Client.Integration.Session
{
    public class MetaTests : IntegrationTest
    {
        public MetaTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper) :
            base(fixture, outputHelper)
        {
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task GetSetMetaPersisted
            (DateTime created, string version)
        {
            Guid id;
            using (var session = SessionFactory.CreateSession())
            {
                var patient = await session.Create<Patient>();
                id = patient.Id;

                patient.Created = created;
                patient.Version = version;

                Assert.Equal(created, patient.Created);
                Assert.Equal(version, patient.Version);
            }

            using (var session = SessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(id);

                Assert.Equal(DateTime.MinValue, patient.Created);
                Assert.Null(patient.Version);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task CreateGetDeletePatient()
        {
            var initialCreated = DateTime.UtcNow;
            var initialVersion = "abcdef123456789";

            var updatedVersion = "987654321fdecba";

            Guid crossSessionId;

            using (var session = SessionFactory.CreateSession())
            {
                var patient = new Patient
                {
                    Created = initialCreated,
                    Version = initialVersion
                };

                patient = await session.Create(patient);

                Assert.NotEqual(Guid.Empty, patient.Id);
                Assert.Equal(initialCreated, patient.Created.ToUniversalTime());
                Assert.Equal(initialVersion, patient.Version);

                crossSessionId = patient.Id;

                var patientRef = await session.Get<Patient>(crossSessionId);

                Assert.Same(patient, patientRef);
            }
            // update!
            using (var session = SessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(crossSessionId);

                Assert.Equal(crossSessionId, patient.Id);
                Assert.Equal(initialCreated, patient.Created.ToUniversalTime());
                Assert.Equal(initialVersion, patient.Version);

                patient.Version = updatedVersion;

                Assert.Equal(updatedVersion, patient.Version);

                await session.Update(patient);
                Assert.Equal(initialCreated, patient.Created.ToUniversalTime());
                Assert.Equal(updatedVersion, patient.Version);

                var patient2 = await session.Get<Patient>(crossSessionId);

                Assert.Same(patient, patient2);
            }
            // later that day...
            using (var session = SessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(crossSessionId);

                Assert.Equal(crossSessionId, patient.Id);
                Assert.Equal(initialCreated, patient.Created.ToUniversalTime());
                Assert.Equal(updatedVersion, patient.Version);
            }
            // cleanup
            using (var session = SessionFactory.CreateSession())
            {
                await session.Delete<Patient>(crossSessionId);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task EnsureUpdateOfExistingWidgetOnCreate()
        {
            Guid id;
            using (var session = SessionFactory.CreateSession())
            {
                var widget = new Widget
                {
                    Sku = "abc123"
                };

                widget = await session.Create(widget);

                Assert.NotEqual(Guid.Empty, widget.Id);

                id = widget.Id;

                Assert.NotNull(widget.ETag);
            }

            // cleanup
            using (var session = SessionFactory.CreateSession())
            {
                await session.Delete<Widget>(id);
            }
        }
    }
}