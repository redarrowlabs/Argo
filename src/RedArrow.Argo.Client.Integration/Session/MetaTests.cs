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
            (DateTime contactTime, string version)
        {
            Guid id;
            using (var session = SessionFactory.CreateSession())
            {
                var patient = await session.Create<Patient>();
                id = patient.Id;

                patient.ContactTime = contactTime;
                patient.Version = version;

                Assert.Equal(contactTime, patient.ContactTime);
                Assert.Equal(version, patient.Version);
            }

            using (var session = SessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(id);

                Assert.Equal(DateTime.MinValue, patient.ContactTime);
                Assert.Null(patient.Version);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task CreateGetDeletePatient()
        {
            var initialContactTime = DateTime.UtcNow;
            var initialVersion = "abcdef123456789";

            var updatedVersion = "987654321fdecba";

            Guid crossSessionId;

            using (var session = SessionFactory.CreateSession())
            {
                var patient = new Patient
                {
                    ContactTime = initialContactTime,
                    Version = initialVersion
                };

                patient = await session.Create(patient);

                Assert.NotEqual(Guid.Empty, patient.Id);
                Assert.Equal(initialContactTime, patient.ContactTime.ToUniversalTime());
                Assert.Equal(initialVersion, patient.Version);

                crossSessionId = patient.Id;

                var patientRef = await session.Get<Patient>(crossSessionId);
                
                Assert.Equal(patient.ContactTime, patientRef.ContactTime);
                Assert.Equal(patient.Version, patientRef.Version);
            }
            // update!
            using (var session = SessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(crossSessionId);

                Assert.Equal(crossSessionId, patient.Id);
                Assert.Equal(initialContactTime, patient.ContactTime.ToUniversalTime());
                Assert.Equal(initialVersion, patient.Version);

                patient.Version = updatedVersion;

                Assert.Equal(updatedVersion, patient.Version);

                await session.Update(patient);
                Assert.Equal(initialContactTime, patient.ContactTime.ToUniversalTime());
                Assert.Equal(updatedVersion, patient.Version);

                var patient2 = await session.Get<Patient>(crossSessionId);

                Assert.Equal(patient.Id, patient2.Id);
                Assert.Equal(patient.ContactTime, patient2.ContactTime);
                Assert.Equal(patient.Version, patient2.Version);
            }
            // later that day...
            using (var session = SessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(crossSessionId);

                Assert.Equal(crossSessionId, patient.Id);
                Assert.Equal(initialContactTime, patient.ContactTime.ToUniversalTime());
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