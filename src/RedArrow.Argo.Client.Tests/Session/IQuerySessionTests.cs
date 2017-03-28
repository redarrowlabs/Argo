using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Cache;
using RedArrow.Argo.Client.Exceptions;
using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Http;
using RedArrow.Argo.Client.Linq.Queryables;
using RedArrow.Argo.Client.Model;
using RedArrow.Argo.Client.Query;
using WovenByFody;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Session
{
    public class IQuerySessionTests : SessionTestsBase
    {
        [Fact]
        public async Task Query__Given_ResourceTypeAndQueryCtx__When_NotFound__Then_ReturnEmpty()
        {
            var uri = "http://www.test.com/";

            var modelRegistry = CreateModelRegistry(typeof(BasicModel));

            var expectedRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(uri));

            var resourceType = modelRegistry.GetResourceType<BasicModel>();
            var include = modelRegistry.GetInclude<BasicModel>();

            var mockRequestBuilder = new Mock<IHttpRequestBuilder>();
            mockRequestBuilder
                .Setup(x => x.QueryResources(It.IsAny<IQueryContext>(), include))
                .Callback<IQueryContext, string>((c, i) =>
                {
                    Assert.NotNull(c);
					Assert.Equal(resourceType, c.BasePath);
					Assert.Equal(include, i);
                })
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

            var result = await subject.Query<BasicModel>(null);

            Assert.NotNull(result);
			Assert.Empty(result);
            
            mockCacheProvider.Verify(x => x.Update(It.IsAny<Guid>(), It.IsAny<object>()), Times.Never);
        }

        [Theory, AutoData]
        public async Task Query__Given_ResourceTypeAndQueryCtx__When_CtxNull__Then_QueryByType
            (Guid modelId)
        {
            var uri = "http://www.test.com/";
            
            var modelRegistry = CreateModelRegistry(typeof(BasicModel));

            var expectedRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(uri));

            var resourceType = modelRegistry.GetResourceType<BasicModel>();
            var include = modelRegistry.GetInclude<BasicModel>();

            var mockRequestBuilder = new Mock<IHttpRequestBuilder>();
            mockRequestBuilder
                .Setup(x => x.QueryResources(It.IsAny<IQueryContext>(), include))
                .Callback<IQueryContext, string>((c, i) =>
                {
                    Assert.NotNull(c);
					Assert.Equal(resourceType, c.BasePath);
					Assert.Equal(include, i);
                })
                .Returns(expectedRequest);

            var mockHandler = new MockRequestHandler();
            mockHandler.Setup(
                new Uri(uri),
                request =>
                {
                    Assert.Same(expectedRequest, request);
                    var root = new ResourceRootCollection
                    {
                        Data = new [] {new Resource {Id = modelId, Type = resourceType } },
                        Included = Enumerable.Range(0, 3).Select(i => new Resource {Id = Guid.NewGuid(), Type = resourceType })
                    };
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(root.ToJson())
                    });
                });

            var mockCacheProvider = new Mock<ICacheProvider>();

            var subject = CreateSubject(
                mockHandler,
                mockRequestBuilder.Object,
                mockCacheProvider.Object,
                modelRegistry);

            var result = await subject.Query<BasicModel>(null);
            Assert.NotEmpty(result);
            Assert.Equal(1, result.Count());
            Assert.Equal(modelId, result.First().Id);

            mockCacheProvider.Verify(x => x.Update(modelId, It.IsAny<object>()), Times.Once);
            mockCacheProvider.Verify(x => x.Update(It.IsAny<Guid>(), It.IsAny<object>()), Times.Exactly(4));
        }

	    [Theory, AutoData]
	    public void CreateQuery__Given_ParentModelAndRltnExpression__Then_CreateQueryable
			(Guid modelId)
	    {
			var parentModel = new ComplexModel {Id = modelId};

		    var modelRegistry = CreateModelRegistry(typeof(ComplexModel), typeof(BasicModel));

			var subject = CreateSubject(modelRegistry: modelRegistry);

		    var resourceType = modelRegistry.GetResourceType<ComplexModel>();
		    Expression<Func<ComplexModel, IEnumerable<BasicModel>>> expression = x => x.BasicModels;
		    var rltnName = (expression.Body as MemberExpression).Member.GetJsonName(typeof(HasManyAttribute));
			
		    var result = subject.CreateQuery(parentModel, expression) as TypeQueryable<BasicModel>;
			Assert.NotNull(result);
			Assert.Equal($"{resourceType}/{modelId}/{rltnName}", result.BuildQuery().BasePath);
		}

		[Fact]
		public void CreateQuery__Given_ParentModelAndExpression__When_ModelNull__Then_ThrowArgNull()
		{
			var subject = CreateSubject(modelRegistry: CreateModelRegistry(typeof(ComplexModel), typeof(BasicModel)));

			Expression<Func<ComplexModel, IEnumerable<BasicModel>>> expression = x => x.BasicModels;

			Assert.Throws<ArgumentNullException>(() => subject.CreateQuery(null, expression));
		}

		[Fact]
	    public void CreateQuery__Given_ParentModelAndExpression__When_PropertyNotHasOne__Then_ThrowNotSupported()
	    {
		    var subject = CreateSubject();

			var model = new ComplexModel();

		    Assert.Throws<RelationshipNotRegisteredExecption>(() => subject.CreateQuery(model, x => x.PropertyB));
		}

		[Fact]
		public void CreateQuery__Given_ParentModelAndExpression__When_ExpressionNotLambda__Then_ThrowNotSupported()
		{
			var subject = CreateSubject();

			var model = new ComplexModel();

			Assert.Throws<NotSupportedException>(() => subject.CreateQuery(model, x => x.BasicModels.Where(y => y.PropA == "")));
		}
	}
}
