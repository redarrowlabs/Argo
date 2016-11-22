using System;
using System.Collections.Generic;
using System.Net.Http;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Jsorm.Attributes;
using RedArrow.Jsorm.Config;
using RedArrow.Jsorm.Session;
using Xunit;

namespace RedArrow.Jsorm.Tests.Config
{
	public class SessionConfigurationTests
	{
		[Theory, AutoData]
		public void CreateGetRequest__Given_Configured__Then_CreateGet
			(Guid patientId)
		{
			var subject = (Fluently.Configure()
				.Models()
				.Configure(x => x.Add<TestParent>())
				.Configure(x => x.Add<TestChild>())
				.BuildSessionFactory() as SessionFactory)
				?.BuildConfiguration();

			Assert.NotNull(subject);

			var result = subject.CreateGetRequest<TestParent>(patientId);

			Assert.NotNull(result);
			Assert.Equal(HttpMethod.Get, result.Method);
			Assert.Equal(
				$"testParent/{patientId}?include=eagerChild",
				result.RequestUri.ToString());
		}
	}

	[Model]
	public class TestParent
	{
		[Id]
		public Guid Id { get; set; }

		[HasOne]
		public TestChild LazyChild { get; set; }
		[HasOne(LoadStrategy.Eager)]
		public TestChild EagerChild { get; set; }

		[HasMany]
		public IEnumerable<TestChild> Children { get; set; }
	}

	[Model]
	public class TestChild
	{
		[Id]
		public Guid Id { get; set; }

		[Property]
		public string Name { get; set; }
	}
}
