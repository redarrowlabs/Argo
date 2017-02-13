using System;
using System.Linq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Session.Registry;
using WovenByFody;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Session.Registry
{
    public class ModelRegistryTests
    {
        [Theory, AutoData]
        public void SetModelId__Given__WovenModel__When_IdPropertyPrivate__Then_SetId
            (Guid id)
        {
            var config = new ModelConfiguration(typeof(ModelWithPrivateIdSetter));

            var subject = new ModelRegistry(new [] {config});

            var model = new ModelWithPrivateIdSetter();

            subject.SetModelId(model, id);

            Assert.Equal(id, model.Id);
        }

        [Theory, AutoData]
        public void SetModelId__Given__WovenModel__When_IdPropertyPublic__Then_SetId
            (Guid id)
        {
            var config = new ModelConfiguration(typeof(ModelWithPublicIdSetter));

            var subject = new ModelRegistry(new[] {config});

            var model = new ModelWithPublicIdSetter();

            subject.SetModelId(model, id);

            Assert.Equal(id, model.Id);
        }

        [Theory, AutoData]
        public void SetModelId__Given__WovenModel__When_NoIdProperty__Then_SetId
            (Guid id)
        {
            var config = new ModelConfiguration(typeof(ModelWithNoIdSetter));

            var subject = new ModelRegistry(new[] {config});

            var model = new ModelWithNoIdSetter();

            subject.SetModelId(model, id);

            Assert.Equal(id, model.Id);
        }

        [Theory, AutoData]
        public void GetModelId__Given__WovenModel__When_IdSet__Then_GetId
            (Guid id)
        {
            var config = new ModelConfiguration(typeof(ModelWithPublicIdSetter));

            var subject = new ModelRegistry(new [] {config});

            var model = new ModelWithPublicIdSetter
            {
                Id = id
            };

            var result = subject.GetModelId(model);

            Assert.Equal(id, result);
        }

        [Fact]
        public void GetModelAttributes__Given_WovenModel__GetConfig()
        {
            var config = new ModelConfiguration(typeof(TestWovenModel));

            var subject = new ModelRegistry(new[] {config});

            var result = subject.GetModelAttributes(typeof(TestWovenModel));

            var camelizedConfig = result.Single(x => x.Property.Name == "CamelizedProperty");
            var customizedConfig = result.Single(x => x.Property.Name == "CustomizedProperty");

            Assert.Equal("camelizedProperty", camelizedConfig.AttributeName);
            Assert.Equal("customized-property", customizedConfig.AttributeName);
        }

        [Fact]
        public void GetResourceType__Given_WovenModel__Then_GetType()
        {
            var config = new ModelConfiguration(typeof(TestWovenModel));

            var subject = new ModelRegistry(new[] {config});

            var result1 = subject.GetResourceType(typeof(TestWovenModel));
            var result2 = subject.GetResourceType<TestWovenModel>();

            Assert.Equal("testWovenModel", result1);
            Assert.Equal("testWovenModel", result2);
        }

        [Fact]
        public void GetResourceType__Given_CustomWovenModel__Then_GetType()
        {
            var config = new ModelConfiguration(typeof(TestCustomWovenModel));

            var subject = new ModelRegistry(new[] {config});

            var result1 = subject.GetResourceType(typeof(TestCustomWovenModel));
            var result2 = subject.GetResourceType<TestCustomWovenModel>();

            Assert.Equal("customized-model-name", result1);
            Assert.Equal("customized-model-name", result2);
        }
    }
}
