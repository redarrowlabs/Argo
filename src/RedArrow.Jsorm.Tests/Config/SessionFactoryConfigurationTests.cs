using RedArrow.Jsorm.Attributes;
using RedArrow.Jsorm.Config;
using RedArrow.Jsorm.Session;
using System;
using System.Linq;
using Xunit;

namespace RedArrow.Jsorm.Tests.Config
{
    public class SessionFactoryConfigurationTests
    {
        [Fact]
        public void BuildSessionFactory__Given_PreConfigured__Then_ReturnConfiguredSessionFactory()
        {
            var subject = new SessionFactoryConfiguration();

            var configurator = new ModelConfiguration();
            configurator.Add<BasicModel>();
            configurator.Add<OverriddenModel>();
            configurator.Configure(subject);

            var result = subject.BuildSessionFactory();

            Assert.IsType<SessionFactory>(result);

            var castResult = result as SessionFactory;

            // type lookup
            Assert.Equal(2, castResult.TypeLookup.Count);
            Assert.True(castResult.TypeLookup.ContainsKey(typeof(BasicModel)));
            Assert.True(castResult.TypeLookup.ContainsKey(typeof(OverriddenModel)));
            Assert.Equal("basicModel", castResult.TypeLookup[typeof(BasicModel)]);
            Assert.Equal("model-overridden", castResult.TypeLookup[typeof(OverriddenModel)]);

            // attribute lookup
            Assert.Equal(2, castResult.AttributeLookup.Count);
            // basic model attributes
            Assert.Equal(3, castResult.AttributeLookup[typeof(BasicModel)].Count());
            Assert.NotNull(castResult.AttributeLookup[typeof(BasicModel)].SingleOrDefault(x => x.AttributeName == "propA"));
            Assert.NotNull(castResult.AttributeLookup[typeof(BasicModel)].SingleOrDefault(x => x.PropertyInfo.Name == "PropA"));
            Assert.NotNull(castResult.AttributeLookup[typeof(BasicModel)].SingleOrDefault(x => x.AttributeName == "propB"));
            Assert.NotNull(castResult.AttributeLookup[typeof(BasicModel)].SingleOrDefault(x => x.PropertyInfo.Name == "PropB"));
            Assert.NotNull(castResult.AttributeLookup[typeof(BasicModel)].SingleOrDefault(x => x.AttributeName == "propC"));
            Assert.NotNull(castResult.AttributeLookup[typeof(BasicModel)].SingleOrDefault(x => x.PropertyInfo.Name == "PropC"));
            // overridden model attributes
            Assert.Equal(1, castResult.AttributeLookup[typeof(OverriddenModel)].Count());
            Assert.NotNull(castResult.AttributeLookup[typeof(OverriddenModel)].SingleOrDefault(x => x.AttributeName == "name-overridden"));
            Assert.NotNull(castResult.AttributeLookup[typeof(OverriddenModel)].SingleOrDefault(x => x.PropertyInfo.Name == "Name"));
        }

        [Model]
        public class BasicModel
        {
            [Id]
            public Guid Id { get; }

            [Property]
            public string PropA { get; set; }

            [Property]
            public int PropB { get; set; }

            [Property]
            public string PropC { get; set; }
        }

        [Model("model-overridden")]
        public class OverriddenModel
        {
            [Id]
            public Guid OverriddenId { get; }

            [Property("name-overridden")]
            public string Name { get; set; }
        }
    }
}