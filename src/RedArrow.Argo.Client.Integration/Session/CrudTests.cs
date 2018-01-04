using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.TestUtils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Client.Integration.Session
{
    public class CrudTests : IntegrationTest
    {
        public CrudTests(IntegrationTestFixture fixture, ITestOutputHelper outputHelper) :
            base(fixture, outputHelper)
        {
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public void GetSetAttributesTransient
            (string firstName, string lastName)
        {
            var patient = new Patient
            {
                FirstName = firstName,
                LastName = lastName
            };

            Assert.Equal(firstName, patient.FirstName);
            Assert.Equal(lastName, patient.LastName);
        }

        [Theory, AutoData, Trait("Category", "Integration")]
        public async Task GetSetAttributesPersisted
            (string firstName, string lastName)
        {
            Guid id;
            using (var session = SessionFactory.CreateSession())
            {
                var patient = await session.Create<Patient>();
                id = patient.Id;

                patient.FirstName = firstName;
                patient.LastName = lastName;

                Assert.Equal(firstName, patient.FirstName);
                Assert.Equal(lastName, patient.LastName);
            }

            using (var session = SessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(id);

                Assert.Null(patient.FirstName);
                Assert.Null(patient.LastName);
            }

            // cleanup
            using (var session = SessionFactory.CreateSession())
            {
                await session.Delete<Patient>(id);
            }
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

                await session.Create(patient);

                // The Create call should also mutate the passed in model with new Meta or attributes
                Assert.NotEqual(DateTime.MinValue, patient.Created);
                Assert.NotNull(patient.Etag);

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

                var oldUpdatedAt = patient.UpdatedAt;
                var oldEtag = patient.Etag;

                await session.Update(patient);
                Assert.Equal(initialFirstName, patient.FirstName);
                Assert.Equal(updatedLastName, patient.LastName);
                // The Update call should also mutate the passed in model with new Meta or attributes
                Assert.NotEqual(oldUpdatedAt, patient.UpdatedAt);
                Assert.NotEqual(oldEtag, patient.Etag);

                var patient2 = await session.Get<Patient>(crossSessionId);

                Assert.Equal(patient.Id, patient2.Id);
                Assert.Equal(patient.FirstName, patient2.FirstName);
                Assert.Equal(patient.LastName, patient2.LastName);
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

        [Fact, Trait("Category", "Integration")]
        public async Task SetAttributesNullPatient()
        {
            Guid crossSessionId;

            using (var session = SessionFactory.CreateSession())
            {
                var patient = new Patient
                {
                    FirstName = "Original",
                    LastName = "Name",
                    Version = "1"
                };

                patient = await session.Create(patient);

                patient.FirstName = null;
                patient.LastName = null;
                patient.Version = null;

                await session.Update(patient);

                crossSessionId = patient.Id;
            }
            using (var session = SessionFactory.CreateSession())
            {
                var patient = await session.Get<Patient>(crossSessionId);

                Assert.Equal(crossSessionId, patient.Id);

                Assert.Null(patient.FirstName);
                Assert.Null(patient.LastName);
                Assert.Null(patient.Version);
            }
            // cleanup
            using (var session = SessionFactory.CreateSession())
            {
                await session.Delete<Patient>(crossSessionId);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task DictionaryDeserializedProperly()
        {
            Guid id;
            using (var session = SessionFactory.CreateSession())
            {
                var patient = new Patient
                {
                    FirstName = "Random",
                    LastName = "Patient",
                    RandomStuff = new Dictionary<string, string>()
                    {
                        { "AwesomeKey", "Awesome Value" }
                    }
                };

                var result = await session.Create(patient);
                id = result.Id;
            }

            try
            {
                using (var session = SessionFactory.CreateSession())
                {
                    var patient = await session.Get<Patient>(id);

                    Assert.Single(patient.RandomStuff.Keys);
                }
            }
            finally
            {
                // cleanup
                using (var session = SessionFactory.CreateSession())
                {
                    await session.Delete<Patient>(id);
                }
            }
        }
    }
}
