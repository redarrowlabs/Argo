using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Cache;
using RedArrow.Argo.Client.Config;
using RedArrow.Argo.Client.Config.Model;
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
    public class SessionTests
    {
        public SessionTests(ITestOutputHelper outputHelper)
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
                    .Create(() => new HttpClient(fakeHandler))
                    .Configure(x => x.BaseAddress = new Uri(basePath))
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

            var fakeHandler = new MockRequestHandler();
			fakeHandler.Setup(
				new Uri($"{basePath}/{resourceType}"),
				x => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)));

			var sessionFactory = Fluently.Configure(basePath)
                .Remote()
                    .Create(() => new HttpClient(fakeHandler))
                    .Configure(x => x.BaseAddress = new Uri(basePath))
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
				new Uri($"{uri}"),
				request =>
				{
					Assert.Same(expectedRequest, request);
					return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
				});

		    var mockCacheProvider = new Mock<ICacheProvider>();
		    mockCacheProvider
			    .Setup(x => x.Retrieve(expectedModelId))
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
				new Uri($"{uri}"),
				request =>
				{
					Assert.Same(expectedRequest, request);
					return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted));
				});

			var mockCacheProvider = new Mock<ICacheProvider>();
			mockCacheProvider
				.Setup(x => x.Retrieve(modelId))
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
            var fakeHandler = new MockRequestHandler();

            var sessionFactory = Fluently.Configure("http://mock.test.com")
                .Remote()
                    .Create(() => new HttpClient(fakeHandler, true))
                .BuildSessionFactory();

            var session = sessionFactory.CreateSession();
            session.Dispose();

            Assert.True(((Client.Session.Session) session).Disposed);
            Assert.True(fakeHandler.Disposed);
        }

	    private Client.Session.Session CreateSubject(
		    MockRequestHandler mockHandler,
			IHttpRequestBuilder requestBuilder = null,
			ICacheProvider cacheProvider = null,
			IModelRegistry modelRegistry = null)
	    {
		    return new Client.Session.Session(
				() => new HttpClient(mockHandler),
				requestBuilder ?? Mock.Of<IHttpRequestBuilder>(),
				cacheProvider ?? Mock.Of<ICacheProvider>(),
				modelRegistry ?? Mock.Of<IModelRegistry>());
	    }

	    private IModelRegistry CreateModelRegistry(params Type[] types)
	    {
		    return new ModelRegistry(types.Select(x => new ModelConfiguration(x)));
	    }
    }
}