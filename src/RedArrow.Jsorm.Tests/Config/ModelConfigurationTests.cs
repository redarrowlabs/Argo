using RedArrow.Jsorm.Attributes;
using RedArrow.Jsorm.Config;
using System;
using System.Linq;
using Xunit;

namespace RedArrow.Jsorm.Tests.Config
{
    public class ModelConfigurationTests
    {
        [Fact]
        public void Configure__Given_AttributedModels__Then_MapToSessionConfiguration()
        {
            var config = new SessionConfiguration();

            var subject = new ModelConfiguration();
            subject.Add<BasicModel>();
            subject.Add<OverriddenModel>();

            subject.Configure(config);

            // types
            Assert.Equal(2, config.Types.Count);

            Assert.True(config.Types.ContainsKey(typeof(BasicModel)));
            Assert.True(config.Types.ContainsKey(typeof(OverriddenModel)));
            Assert.Equal("basicModel", config.Types[typeof(BasicModel)]);
            Assert.Equal("model-overridden", config.Types[typeof(OverriddenModel)]);

            // ids
            Assert.Equal(2, config.IdProperties.Count());
            Assert.NotNull(config.IdProperties.SingleOrDefault(x => x.Name == "Id"));
            Assert.NotNull(config.IdProperties.SingleOrDefault(x => x.Name == "OverriddenId"));

            // attributes
            Assert.Equal(4, config.AttributeProperties.Count());
            Assert.NotNull(config.AttributeProperties.SingleOrDefault(x => x.PropertyInfo.Name == "PropA"));
            Assert.NotNull(config.AttributeProperties.SingleOrDefault(x => x.PropertyInfo.Name == "PropB"));
            Assert.NotNull(config.AttributeProperties.SingleOrDefault(x => x.PropertyInfo.Name == "PropC"));
            Assert.NotNull(config.AttributeProperties.SingleOrDefault(x => x.PropertyInfo.Name == "Name"));
        }

        [Fact]
        public void Add__Given_AttributedModels__When_RedundantAdd__Then_SingleAddition()
        {
            var config = new SessionConfiguration();

            var subject = new ModelConfiguration();
            subject.Add<BasicModel>();
            subject.Add<OverriddenModel>();
            subject.Add(typeof(BasicModel));
            subject.Add(typeof(OverriddenModel));

            subject.Configure(config);

            Assert.Equal(1, subject.ModelTypes.Count(x => x == typeof(BasicModel)));
            Assert.Equal(1, subject.ModelTypes.Count(x => x == typeof(OverriddenModel)));
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