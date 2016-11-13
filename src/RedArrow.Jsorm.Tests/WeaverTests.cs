using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RedArrow.Jsorm.Logging.LogProviders;
using RedArrow.Jsorm.Session;
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
        public void ModelsHavePrivateSessionField()
        {
	        Assert.All(WovenTypes(), type =>
			{
				var field = type.GetFieldsPortable()
					.Where(x => x.IsPrivate)
					.Where(x => x.Name == "_jsorm_generated_session")
					.Where(x => x.IsNotSerialized)
					.SingleOrDefault(x => x.FieldType == typeof(ISession));

				Assert.NotNull(field);
			});
        }

	    [Fact]
	    public void ModelsHaveNonPublicIdSetter()
	    {
		    Assert.All(WovenTypes(), type =>
		    {
			    var idProp = type.GetTypeInfo().GetProperties()
				    .SingleOrDefault(x => x.Name == "Id");

				Assert.NotNull(idProp.SetMethod);
				Assert.False(idProp.SetMethod.IsPublic);
		    });
	    }

	    [Fact]
	    public void ModelsHaveSessionArgCtor()
	    {
		    Assert.All(WovenTypes(), type =>
		    {
			    var ctor = type.GetTypeInfo().GetConstructors()
				    .Where(x => x.IsPublic)
				    .Where(x => x.GetParameters().Count() == 1)
				    .SingleOrDefault(x => x.GetParameters()[0].ParameterType == typeof (ISession));

				Assert.NotNull(ctor);
		    });
	    }

	    private IEnumerable<Type> WovenTypes()
	    {
		    return new[]
		    {
				Fixture.WovenAssembly.GetType("AssemblyToWeave.Patient"),
				Fixture.WovenAssembly.GetType("AssemblyToWeave.Provider")
			};
	    }
    }
}