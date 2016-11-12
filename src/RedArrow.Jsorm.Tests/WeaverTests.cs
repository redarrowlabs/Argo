using System;
using System.Linq;
using Xunit;

namespace RedArrow.Jsorm.Tests
{
    public class WeaverTests : IClassFixture<WeaverTestFixture>
    {
        private WeaverTestFixture Fixture { get; }

        public WeaverTests(WeaverTestFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public void HelloWorld()
        {
            var patientType = Fixture.WovenAssembly.GetType("AssemblyToWeave.Patient");
            var ctor = patientType.GetConstructors()
                .SingleOrDefault(x =>
                {
                    var ctorParams = x.GetParameters();
                    return ctorParams.Length == 1 &&
                           ctorParams[0].ParameterType.FullName == "RedArrow.Jsorm.Session.ISession";
                });

            Assert.NotNull(ctor);
        }
    }
}