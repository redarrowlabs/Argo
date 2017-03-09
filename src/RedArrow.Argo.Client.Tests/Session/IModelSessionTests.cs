using System;
using Moq;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Cache;
using RedArrow.Argo.Client.Http;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Tests.Extensions;
using WovenByFody;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Session
{
    public class IModelSessionTests : SessionTestsBase
    {
        [Theory, AutoData]
        public void SetReference__Given_SessionManagedModel__When_RefNull__Then_SetNullValue
            (Guid modelId)
        {
            var model = new CircularReferenceA { Id = modelId };

            var mockRequestBuilder = new Mock<IHttpRequestBuilder>();
            var mockCacheProvider = new Mock<ICacheProvider>();
            var modelRegistry = CreateModelRegistry(
                 typeof(CircularReferenceA),
                 typeof(CircularReferenceB));

            var subject = CreateSubject(
                null,
                mockRequestBuilder.Object,
                mockCacheProvider.Object,
                modelRegistry);

            model = subject.ManageModel(model);

            subject.SetReference(model, "b", (CircularReferenceB)null);

            var patch = modelRegistry.GetPatch(model);
            var resource = modelRegistry.GetResource(model);

            Assert.NotNull(patch);
            Assert.Equal(modelId, patch.Id);
            Assert.Equal(modelRegistry.GetResourceType(typeof(CircularReferenceA)), patch.Type);
            Assert.Null(patch.Attributes);
            Assert.NotNull(patch.Relationships);
            Assert.Contains("b", patch.Relationships.Keys);
            Assert.NotNull(patch.Relationships["b"].Data);
            Assert.Equal(JTokenType.Null, patch.Relationships["b"].Data.Type);

            Assert.NotNull(resource);
            Assert.Null(resource.Relationships);

            mockCacheProvider.Verify(x => x.Update(It.IsAny<Guid>(), It.IsAny<object>()), Times.Never);
        }

        [Theory, AutoData]
        public void SetReference__Given_SessionManagedModel__When_RefModelWithId__Then_CreateRelationship
            (Guid modelId, Guid refId)
        {
            var model = new CircularReferenceA { Id = modelId };
            var refModel = new CircularReferenceB { Id = refId };

            var mockRequestBuilder = new Mock<IHttpRequestBuilder>();
            var mockCacheProvider = new Mock<ICacheProvider>();
            var modelRegistry = CreateModelRegistry(
                 typeof(CircularReferenceA),
                 typeof(CircularReferenceB));

            var subject = CreateSubject(
                null,
                mockRequestBuilder.Object,
                mockCacheProvider.Object,
                modelRegistry);

            model = subject.ManageModel(model);

            subject.SetReference(model, "b", refModel);

            var patch = modelRegistry.GetPatch(model);
            var resource = modelRegistry.GetResource(model);

            Assert.NotNull(patch);
            Assert.Equal(modelId, patch.Id);
            Assert.Equal(modelRegistry.GetResourceType<CircularReferenceA>(), patch.Type);
            Assert.Null(patch.Attributes);
            Assert.NotNull(patch.Relationships);
            Assert.Contains("b", patch.Relationships.Keys);
            Assert.NotNull(patch.Relationships["b"].Data);
            Assert.Equal(JTokenType.Object, patch.Relationships["b"].Data.Type);
            var refIdentifier = patch.Relationships["b"].Data.ToObject<ResourceIdentifier>();
            Assert.Equal(refId, refIdentifier.Id);
            Assert.Equal(modelRegistry.GetResourceType<CircularReferenceB>(), refIdentifier.Type);

            Assert.NotNull(resource);
            Assert.Null(resource.Relationships);

            mockCacheProvider.Verify(x => x.Update(refModel.Id, refModel), Times.Once);
        }

        [Theory, AutoData]
        public void SetReference__Given_SessionManagedModel__When_RefModelWithoutId__Then_CreateRelationship
            (Guid modelId)
        {
            var model = new CircularReferenceA { Id = modelId };
            var refModel = new CircularReferenceB();

            var mockRequestBuilder = new Mock<IHttpRequestBuilder>();
            var mockCacheProvider = new Mock<ICacheProvider>();
            var modelRegistry = CreateModelRegistry(
                 typeof(CircularReferenceA),
                 typeof(CircularReferenceB));

            var subject = CreateSubject(
                null,
                mockRequestBuilder.Object,
                mockCacheProvider.Object,
                modelRegistry);

            model = subject.ManageModel(model);

            subject.SetReference(model, "b", refModel);

            var patch = modelRegistry.GetPatch(model);
            var resource = modelRegistry.GetResource(model);

            Assert.NotNull(patch);
            Assert.Equal(modelId, patch.Id);
            Assert.Equal(modelRegistry.GetResourceType<CircularReferenceA>(), patch.Type);
            Assert.Null(patch.Attributes);
            Assert.NotNull(patch.Relationships);
            Assert.Contains("b", patch.Relationships.Keys);
            Assert.NotNull(patch.Relationships["b"].Data);
            Assert.Equal(JTokenType.Object, patch.Relationships["b"].Data.Type);
            var refIdentifier = patch.Relationships["b"].Data.ToObject<ResourceIdentifier>();
            Assert.NotEqual(Guid.Empty, refIdentifier.Id);
            Assert.NotEqual(Guid.Empty, refModel.Id);
            Assert.Equal(refModel.Id, refIdentifier.Id);
            Assert.Equal(modelRegistry.GetResourceType<CircularReferenceB>(), refIdentifier.Type);

            Assert.NotNull(resource);
            Assert.Null(resource.Relationships);

            mockCacheProvider.Verify(x => x.Update(refModel.Id, refModel), Times.Once);
        }
    }
}
