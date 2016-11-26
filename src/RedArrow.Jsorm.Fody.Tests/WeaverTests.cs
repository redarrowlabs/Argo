﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy.Internal;
using RedArrow.Jsorm.Session;
using Xunit;

namespace RedArrow.Jsorm.Fody.Tests
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
				var field = type.GetAllFields()
					.Where(x => x.IsPrivate)
					.Where(x => x.Name == "__jsorm__generated_session")
					.Where(x => x.IsNotSerialized)
					.SingleOrDefault(x => x.FieldType == typeof(IModelSession));

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
				    .Where(x => x.GetParameters().Count() == 2)
					.Where(x => x.GetParameters()[0].ParameterType == typeof(Guid))
				    .SingleOrDefault(x => x.GetParameters()[1].ParameterType == typeof (IModelSession));

				Assert.NotNull(ctor);
		    });
		}

		private IEnumerable<Type> WovenTypes()
	    {
            return new[]
            {
                Fixture.WovenAssembly.GetType("WovenByTest.Patient"),
                Fixture.WovenAssembly.GetType("WovenByTest.Provider")
            };
	    }
    }
}