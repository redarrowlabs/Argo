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

            var subject = new ModelRegistry(new[] {config});

            var model = new ModelWithPrivateIdSetter();

            subject.SetId(model, id);

            Assert.Equal(id, model.Id);
        }

        [Theory, AutoData]
        public void SetModelId__Given__WovenModel__When_IdPropertyPublic__Then_SetId
            (Guid id)
        {
            var config = new ModelConfiguration(typeof(ModelWithPublicIdSetter));

            var subject = new ModelRegistry(new[] {config});

            var model = new ModelWithPublicIdSetter();

            subject.SetId(model, id);

            Assert.Equal(id, model.Id);
        }

        [Theory, AutoData]
        public void SetModelId__Given__WovenModel__When_NoIdProperty__Then_SetId
            (Guid id)
        {
            var config = new ModelConfiguration(typeof(ModelWithNoIdSetter));

            var subject = new ModelRegistry(new[] {config});

            var model = new ModelWithNoIdSetter();

            subject.SetId(model, id);

            Assert.Equal(id, model.Id);
        }

        [Theory, AutoData]
        public void GetModelId__Given__WovenModel__When_IdSet__Then_GetId
            (Guid id)
        {
            var config = new ModelConfiguration(typeof(ModelWithPublicIdSetter));

            var subject = new ModelRegistry(new[] {config});

            var model = new ModelWithPublicIdSetter
            {
                Id = id
            };

            var result = subject.GetId(model);

            Assert.Equal(id, result);
        }

        [Fact]
        public void GetModelAttributes__Given_WovenModel__GetConfig()
        {
            var config = new ModelConfiguration(typeof(TestWovenModel));

            var subject = new ModelRegistry(new[] {config});

            var result = subject.GetAttributeConfigs(typeof(TestWovenModel));

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

        [Fact]
        public void GetIncludedModels__Given_Model__When_RelatedNull__Then_ReturnModels()
        {
            var subject = CreateSubject(
                typeof(CircularReferenceA),
                typeof(CircularReferenceB),
                typeof(CircularReferenceC),
                typeof(CircularReferenceD));

            var a = new CircularReferenceA();
            var b = new CircularReferenceB();
            var c = new CircularReferenceC();

            var ds = Enumerable.Range(0, 3)
                .Select(x => new CircularReferenceD())
                .ToArray();

            a.B = b;

            b.A = a;
            b.C = c;

            c.A = a;
            c.PrimaryD = ds.First();
            c.AllDs = ds;

            var result = subject.GetIncludedModels(a);

            Assert.NotNull(result);
            Assert.NotEmpty(result);

            Assert.Equal(6, result.Count());

            Assert.Contains(a, result);
            Assert.Contains(b, result);
            Assert.Contains(c, result);
            Assert.All(ds, d => Assert.Contains(d, result));
        }

        [Fact]
        public void GetIncludedModels__Given_Null__Then_ReturnNull()
        {
            var subject = CreateSubject(typeof(CircularReferenceA));

            var result = subject.GetIncludedModels(null);

            Assert.Null(result);
        }

        [Fact]
        public void GetIncludedModels__Given_NullHasOneReference__Then_ReturnModels()
        {
            var subject = CreateSubject(
                typeof(CircularReferenceA),
                typeof(CircularReferenceB));

            var a = new CircularReferenceA();
            var b = new CircularReferenceB();

            a.B = b;

            b.A = a;
            b.C = null;

            var result = subject.GetIncludedModels(a);

            Assert.NotNull(result);
            Assert.NotEmpty(result);

            Assert.Equal(2, result.Count());

            Assert.Contains(a, result);
            Assert.Contains(b, result);
        }

        [Fact]
        public void GetIncludedMOdels__Given_NullHasManyReference__Then_ReturnModels()
        {
            var subject = CreateSubject(
                typeof(CircularReferenceA),
                typeof(CircularReferenceB),
                typeof(CircularReferenceC));

            var a = new CircularReferenceA();
            var b = new CircularReferenceB();
            var c = new CircularReferenceC();
            
            a.B = b;

            b.A = a;
            b.C = c;

            c.A = a;
            c.PrimaryD = null;
            c.AllDs = null;

            var result = subject.GetIncludedModels(a);

            Assert.NotNull(result);
            Assert.NotEmpty(result);

            Assert.Equal(3, result.Count());

            Assert.Contains(a, result);
            Assert.Contains(b, result);
            Assert.Contains(c, result);
        }

        private static ModelRegistry CreateSubject(params Type[] modelTypes)
        {
            return new ModelRegistry(modelTypes.Select(x => new ModelConfiguration(x)));
        }
    }
}
