using System;
using System.Linq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Session.Registry;
using RedArrow.Argo.Client.Tests.Session.Models;
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

            var result = subject.GetIncluded(a);

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

            var result = subject.GetIncluded(null);

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

            var result = subject.GetIncluded(a);

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

            var result = subject.GetIncluded(a);

            Assert.NotNull(result);
            Assert.NotEmpty(result);

            Assert.Equal(3, result.Count());

            Assert.Contains(a, result);
            Assert.Contains(b, result);
            Assert.Contains(c, result);
        }

        [Theory, AutoData]
        public void GetAttributeValues__Given_Model__Then_ReturnAllAttributeValues
            (string expectedAttr1, int expectedAttr2, long expectedAttr3)
        {
            var subject = CreateSubject(typeof(TestGetModelAttributeValues));

            var model = new TestGetModelAttributeValues
            {
                Attribute1 = expectedAttr1,
                Attribute2 = expectedAttr2,
                Attribute3 = expectedAttr3
            };

            var result = subject.GetAttributeValues(model);

            Assert.Equal(expectedAttr1, result.Value<string>("attribute1"));
            Assert.Equal(expectedAttr2, result.Value<int>("attribute-2"));
            Assert.Equal(expectedAttr3, result.Value<long>("attribute3"));
        }

        [Theory, AutoData]
        public void GetAttributeValues__Given_Model__When_NullValues__Then_ReturnNonNullAttributeValues
            (string expectedAttr1, long expectedAttr3)
        {
            var subject = CreateSubject(typeof(TestGetModelAttributeValues));

            var model = new TestGetModelAttributeValues
            {
                Attribute1 = expectedAttr1,
                Attribute3 = expectedAttr3
            };

            var result = subject.GetAttributeValues(model);

            Assert.Equal(expectedAttr1, result.Value<string>("attribute1"));
            Assert.Null(result["attribute-2"]);
            Assert.Equal(expectedAttr3, result.Value<long>("attribute3"));
        }

        [Fact]
        public void GetAttributeValues__Given_Model__When_ModelNull__Then_ReturnNull()
        {
            var subject = CreateSubject(typeof(TestGetModelAttributeValues));

            var result = subject.GetAttributeValues(null);

            Assert.Null(result);
        }

        private static ModelRegistry CreateSubject(params Type[] modelTypes)
        {
            return new ModelRegistry(modelTypes.Select(x => new ModelConfiguration(x)));
        }
    }
}
