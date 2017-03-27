﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Cache;
using RedArrow.Argo.Client.Config;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Exceptions;
using RedArrow.Argo.Client.Http;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Session.Registry;
using RedArrow.Argo.Client.Tests.Extensions;
using RedArrow.Argo.Client.Tests.Session.Models;
using RedArrow.Argo.TestUtils.XUnitSink;
using Serilog;
using WovenByFody;
using Xunit;
using Xunit.Abstractions;

namespace RedArrow.Argo.Client.Tests.Session
{
    public class ISessionTests : SessionTestsBase
    {
        public ISessionTests(ITestOutputHelper outputHelper)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.XunitTestOutput(outputHelper)
                .CreateLogger();
        }

        [Fact]
        public async Task Create__WithCollectionAsProperty__Then_PostAndCacheNewModel()
        {
            var basePath = "http://www.test.com";

            var resourceType = "test-property-collection";

            var fakeHandler = new MockRequestHandler();
			fakeHandler.Setup(
				new Uri($"{basePath}/{resourceType}"),
				x => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)));

            var sessionFactory = Fluently.Configure(basePath)
                .Remote()
                    .Configure(x => x.BaseAddress = new Uri(basePath))
					.Configure(builder => builder
						.UseRequestHandler<MockRequestHandler>()
						.ConfigureRequestHandler(x => ((MockRequestHandler)x)
							.Setup(
								new Uri($"{basePath}/{resourceType}"),
								request => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)))))
				.Models()
                    .Configure(x => x.AssemblyOf<ObjectWithCollectionsAndComplexTypesAsProperties>())
                .BuildSessionFactory();

            var session = sessionFactory.CreateSession();
            var result = await session.Create(new ObjectWithCollectionsAndComplexTypesAsProperties
            {
                TestingDescription = new InnerObject
                {
                    Value = "A complex type used as a Property"
                },
                TestingDescriptions = new List<InnerObject>
                {
                    new InnerObject
                    {
                        Value = "A complex type in a collection used as a Property"
                    },
                    new InnerObject
                    {
                        Value = "Value Two"
                    },
                    new InnerObject
                    {
                        Value = "Value Three"
                    }
                }
            });

            Assert.NotNull(result);
            Assert.IsType<ObjectWithCollectionsAndComplexTypesAsProperties>(result);

            Assert.NotEqual(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task Create__Given_NoArgs__Then_PostAndCacheNewModel()
        {
            var basePath = "http://www.test.com";

            var resourceType = "integration-test-patient";
			
			var sessionFactory = Fluently.Configure(basePath)
                .Remote()
                    .Configure(x => x.BaseAddress = new Uri(basePath))
					.Configure(builder => builder
						.UseRequestHandler<MockRequestHandler>()
						.ConfigureRequestHandler(x => ((MockRequestHandler)x)
							.Setup(
								new Uri($"{basePath}/{resourceType}"),
								request => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)))))
				.Models()
                    .Configure(x => x.AssemblyOf<Patient>())
                .BuildSessionFactory();

            var session = sessionFactory.CreateSession();
            var result = await session.Create<Patient>();

            Assert.NotNull(result);
            Assert.IsType<Patient>(result);

            Assert.NotEqual(Guid.Empty, result.Id);
        }

	    [Theory, AutoData]
	    public async Task Create__Given_BasicModel__Then_PostAndCacheModel
			(Guid expectedModelId, string expectedPropA, string expectedPropB)
		{
			var uri = "http://www.test.com/";
			
			var model = new BasicModel
			{
				Id = expectedModelId,
				PropA = expectedPropA,
				PropB = expectedPropB
			};
			
			var expectedRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(uri));
			var expectedResultModel = new BasicModel();

			var mockRequestBuilder = new Mock<IHttpRequestBuilder>();
			mockRequestBuilder
				.Setup(x => x.CreateResource(It.IsAny<Resource>(), It.IsAny<IEnumerable<Resource>>()))
				.Callback<Resource, IEnumerable<Resource>>((resource, includes) =>
				{
					Assert.Equal(expectedModelId, resource.Id);
					Assert.NotNull(resource.Attributes);
					Assert.Equal(2, resource.Attributes.Count);
					Assert.Equal(expectedPropA, resource.Attributes.GetValue("propA").ToObject<string>());
					Assert.Equal(expectedPropB, resource.Attributes.GetValue("propB").ToObject<string>());

					Assert.Null(resource.Relationships);
					Assert.Null(resource.Links);
					Assert.Null(resource.Meta);

					Assert.Empty(includes);
				})
				.Returns(expectedRequest);

			var mockHandler = new MockRequestHandler();
			mockHandler.Setup(
				new Uri(uri),
				request =>
				{
					Assert.Same(expectedRequest, request);
					return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
				});

		    var mockCacheProvider = new Mock<ICacheProvider>();
		    mockCacheProvider
			    .Setup(x => x.Retrieve<BasicModel>(expectedModelId))
			    .Returns(expectedResultModel);

			var modelRegistry = CreateModelRegistry(typeof(BasicModel));

			var subject = CreateSubject(
			    mockHandler,
			    mockRequestBuilder.Object,
			    mockCacheProvider.Object,
				modelRegistry);

		    var result = await subject.Create(model);
			
			Assert.NotNull(result);
			Assert.Same(expectedResultModel, result);

			mockCacheProvider
				.Verify(x => x.Update(expectedModelId, It.IsAny<BasicModel>()), Times.Once);
		}

		[Theory, AutoData]
		public async Task Create__Given_ComplexModel__Then_PostAndCacheModel
			(Guid modelId)
		{
			var uri = "http://www.test.com/";

			var a = new CircularReferenceA {Id = modelId};
			var b = new CircularReferenceB();
			var c = new CircularReferenceC();

			a.B = b;
			b.A = a;
			b.C = c;
			c.A = a;

			var expectedRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(uri));
			var expectedResultModel = new CircularReferenceA();
			
			var mockRequestBuilder = new Mock<IHttpRequestBuilder>();
			mockRequestBuilder
				.Setup(x => x.CreateResource(It.IsAny<Resource>(), It.IsAny<IEnumerable<Resource>>()))
				.Callback<Resource, IEnumerable<Resource>>((resource, includes) =>
				{
					Assert.Equal(modelId, resource.Id);
					Assert.Null(resource.Attributes);

					Assert.NotNull(resource.Relationships);
					Assert.Equal(1, resource.Relationships.Count);

					Assert.NotEmpty(includes);
					Assert.Equal(2, includes.Count());

					Assert.Null(resource.Links);
					Assert.Null(resource.Meta);
				})
				.Returns(expectedRequest);

			var mockHandler = new MockRequestHandler();
			mockHandler.Setup(
				new Uri(uri),
				request =>
				{
					Assert.Same(expectedRequest, request);
					return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
				});

			var mockCacheProvider = new Mock<ICacheProvider>();
			mockCacheProvider
				.Setup(x => x.Retrieve<CircularReferenceA>(modelId))
				.Returns(expectedResultModel);

			var modelRegistry = CreateModelRegistry(
				typeof(CircularReferenceA),
				typeof(CircularReferenceB),
				typeof(CircularReferenceC));

			var subject = CreateSubject(
                mockHandler,
				mockRequestBuilder.Object,
				mockCacheProvider.Object,
				modelRegistry);

			var result = await subject.Create(a);

			Assert.NotNull(result);
			Assert.Same(expectedResultModel, result);

			mockCacheProvider
				.Verify(x => x.Update(It.IsAny<Guid>(), It.IsAny<CircularReferenceA>()), Times.Once);
			mockCacheProvider
				.Verify(x => x.Update(It.IsAny<Guid>(), It.IsAny<CircularReferenceB>()), Times.Once);
			mockCacheProvider
				.Verify(x => x.Update(It.IsAny<Guid>(), It.IsAny<CircularReferenceC>()), Times.Once);
		}

        [Fact]
        public async Task Update__Given_NullModel__Then_ThrowEx()
        {
            var subject = CreateSubject();
            await Assert.ThrowsAsync<ArgumentNullException>(() => subject.Update((BasicModel) null));
        }

        [Fact]
        public async Task Update__Given_DetachedModel__Then_ThrowEx()
        {
            var model = new BasicModel();
            var subject = CreateSubject(modelRegistry: CreateModelRegistry(typeof(BasicModel)));
            await Assert.ThrowsAsync<UnmanagedModelException>(() => subject.Update(model));
        }

        [Theory, AutoData]
        public async Task Update__Given_Model__When_NoPatch__Then_Return
            (Guid modelId)
        {
            var model = new ComplexModel { Id = modelId };
            
            var mockRequestBuilder = new Mock<IHttpRequestBuilder>();

            var mockHandler = new MockRequestHandler();

            var modelRegistry = CreateModelRegistry(
                 typeof(ComplexModel),
                 typeof(BasicModel));

            var mockCacheProvider = new Mock<ICacheProvider>();

            var subject = CreateSubject(
                mockHandler,
                mockRequestBuilder.Object,
                mockCacheProvider.Object,
                modelRegistry);

            model = subject.ManageModel(model);

            await subject.Update(model);

            Assert.Equal(0, mockHandler.RequestsSent);

            mockRequestBuilder.Verify(x => x.UpdateResource(It.IsAny<Resource>(), It.IsAny<IEnumerable<Resource>>()), Times.Never);
            mockCacheProvider.Verify(x => x.Update(It.IsAny<Guid>(), It.IsAny<object>()), Times.Never);
        }

        [Theory, AutoData]
        public async Task Update__Given_Model__Then_SendPatch
            (Guid modelId, string modelPropA, string modelPropB)
        {
            var uri = "http://www.test.com/";

            var model = new ComplexModel {Id = modelId};
            var primaryBasicModel = new BasicModel {PropB = modelPropB};

            var expectedRequest = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri(uri));
            
            var modelRegistry = CreateModelRegistry(
                 typeof(ComplexModel),
                 typeof(BasicModel));

            var mockRequestBuilder = new Mock<IHttpRequestBuilder>();
            mockRequestBuilder
                .Setup(x => x.UpdateResource(It.IsAny<Resource>(), It.IsAny<IEnumerable<Resource>>()))
                .Callback<Resource, IEnumerable<Resource>>((resource, includes) =>
                {
                    Assert.Equal(modelId, resource.Id);
                    
                    Assert.Equal(modelPropA, resource.Attributes?["propertyA"]?.ToObject<string>());
                    Assert.Equal(1, resource.Attributes.Count);

                    Assert.NotNull(resource.Relationships);
                    Assert.Equal(1, resource.Relationships.Count);
                    Assert.Equal(JTokenType.Object, resource.Relationships["primaryBasicModel"]?.Data?.Type);

                    var rltnIdentifier = resource.Relationships["primaryBasicModel"].Data.ToObject<ResourceIdentifier>();
                    Assert.NotEqual(Guid.Empty, rltnIdentifier.Id);
                    Assert.Equal(primaryBasicModel.Id, rltnIdentifier.Id);
                    Assert.Equal(modelRegistry.GetResourceType<BasicModel>(), rltnIdentifier.Type);

                    Assert.NotEmpty(includes);
                    Assert.Equal(1, includes.Count());
                    var include = includes.First();
                    Assert.Equal(include.Id, primaryBasicModel.Id);
                    Assert.Equal(modelRegistry.GetResourceType<BasicModel>(), include.Type);
                    Assert.Equal(modelPropB, include.Attributes?["propB"]?.ToObject<string>());
                    Assert.Null(include.Relationships);

                    Assert.Null(resource.Links);
                    Assert.Null(resource.Meta);
                })
                .Returns(expectedRequest);

            var mockHandler = new MockRequestHandler();
            mockHandler.Setup(
                new Uri(uri),
                request =>
                {
                    Assert.Same(expectedRequest, request);
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
                });

            var cachedItems = new List<object>();

            var mockCacheProvider = new Mock<ICacheProvider>();
            mockCacheProvider
                .Setup(x => x.Update(It.IsAny<Guid>(), It.IsAny<BasicModel>()))
                .Callback<Guid, object>((id, m) =>
                {
                    cachedItems.Add(m);
                    mockCacheProvider
                        .Setup(y => y.Retrieve<BasicModel>(id))
                        .Returns((BasicModel) m);
                });
            mockCacheProvider
                .SetupGet(x => x.Items)
                .Returns(() => cachedItems);

            var subject = CreateSubject(
                mockHandler,
                mockRequestBuilder.Object,
                mockCacheProvider.Object,
                modelRegistry);

            model = subject.ManageModel(model);

            model.PropertyA = modelPropA;
            model.PrimaryBasicModel = primaryBasicModel;

            await subject.Update(model);

            mockCacheProvider.Verify(x => x.Update(model.PrimaryBasicModel.Id, model.PrimaryBasicModel), Times.Once);
            mockCacheProvider.Verify(x => x.Update(model.Id, model), Times.Never);
        }
		
        [Theory, AutoData]
        public async Task Get__Given_ModelId__When_CacheContainsModel__Then_ReturnCachedModel
            (Guid modelId)
        {
            var cachedModel = new BasicModel {Id = modelId};

            var mockCacheProvider = new Mock<ICacheProvider>();
            mockCacheProvider
                .Setup(x => x.Retrieve<BasicModel>(modelId))
                .Returns(cachedModel);

            var subject = CreateSubject(cacheProvider: mockCacheProvider.Object);

            var result = await subject.Get<BasicModel>(modelId);

            Assert.Same(cachedModel, result);
        }

        [Theory, AutoData]
        public async Task Get__Given_ModelId__When_StatusCode404__Then_ReturnNull
            (Guid modelId)
        {
            var uri = "http://www.test.com/";
            var expectedRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(uri));

            var modelRegistry = CreateModelRegistry(typeof(BasicModel));

            var resourceType = modelRegistry.GetResourceType<BasicModel>();

            var mockRequestBuilder = new Mock<IHttpRequestBuilder>();
            mockRequestBuilder
                .Setup(x => x.GetResource(modelId, resourceType, string.Empty))
                .Returns(expectedRequest);

            var mockHandler = new MockRequestHandler();
            mockHandler.Setup(
                new Uri(uri),
                request =>
                {
                    Assert.Same(expectedRequest, request);
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                });

            var mockCacheProvider = new Mock<ICacheProvider>();

            var subject = CreateSubject(
                mockHandler,
                mockRequestBuilder.Object,
                mockCacheProvider.Object,
                modelRegistry);

            var result = await subject.Get<BasicModel>(modelId);
            
            Assert.Null(result);
        }

        [Theory, AutoData]
        public async Task Get__Given_ModelId__When_DataRetrieved__Then_CreateAndCacheModel
            (Guid modelId)
        {
            var uri = "http://www.test.com/";
            var expectedRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(uri));

            var modelRegistry = CreateModelRegistry(typeof(BasicModel));

            var resourceType = modelRegistry.GetResourceType<BasicModel>();

            var mockRequestBuilder = new Mock<IHttpRequestBuilder>();
            mockRequestBuilder
                .Setup(x => x.GetResource(modelId, resourceType, string.Empty))
                .Returns(expectedRequest);

            var mockHandler = new MockRequestHandler();
            mockHandler.Setup(
                new Uri(uri),
                request =>
                {
                    Assert.Same(expectedRequest, request);
                    var root = ResourceRootSingle.FromResource(new Resource {Id = modelId, Type = modelRegistry.GetResourceType<BasicModel>()});
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(root))
                    });
                });

            var mockCacheProvider = new Mock<ICacheProvider>();

            var subject = CreateSubject(
                mockHandler,
                mockRequestBuilder.Object,
                mockCacheProvider.Object,
                modelRegistry);

            var result = await subject.Get<BasicModel>(modelId);

            Assert.NotNull(result);
            Assert.Equal(modelId, result.Id);

            mockCacheProvider.Verify(x => x.Update(modelId, result), Times.Once);
        }

        [Fact]
        public async Task Create__When_Disposed__Then_ThrowEx()
        {
            var session = Fluently.Configure("http://mock.test.com")
                .BuildSessionFactory()
                .CreateSession();

            session.Dispose();

            await Assert.ThrowsAsync<Exception>(() => session.Create<Patient>());
        }

        [Fact]
        public void Dispose__Given_ConfiguredHttpClient__When_Disposed__Then_DisposeClient()
        {
			MockRequestHandler capturedHandler = null;

            var sessionFactory = Fluently.Configure("http://mock.test.com")
                .Remote()
					.Configure(builder => builder
						.UseRequestHandler<MockRequestHandler>()
						.ConfigureRequestHandler(handler => capturedHandler = (MockRequestHandler)handler))
				.BuildSessionFactory();

            var session = sessionFactory.CreateSession();
            session.Dispose();

            Assert.True(((Client.Session.Session) session).Disposed);
			Assert.NotNull(capturedHandler);
			Assert.True(capturedHandler.Disposed);
        }
    }
}