using System.Linq;
using System.Linq.Expressions;
using Moq;
using Newtonsoft.Json;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Linq;
using RedArrow.Argo.Client.Linq.Queryables;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;
using WovenByFody;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Linq.Queryable
{
    public class SkipQueryableTests
    {
        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(25)]
        public void BuildQuery__Given_NumberToSkip__When_NumberGreaterThanZero__Then_SetQueryPaging
            (int skip)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var mockTarget = new Mock<RemoteQueryable<BasicModel>>(Mock.Of<IQuerySession>(), Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            var subject = CreateSubject(mockTarget.Object, Expression.Constant(skip));

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.VerifySet(x => x.PageOffset = skip, Times.Once);

            mockQueryContext.VerifySet(x => x.PageLimit = It.IsAny<int>(), Times.Never());
            mockQueryContext.VerifySet(x => x.PageSize = It.IsAny<int>(), Times.Never());
            mockQueryContext.VerifySet(x => x.PageNumber = It.IsAny<int>(), Times.Never());
        }

        [Theory, AutoData]
        public void ToArray__When_Chained__Then_BuildQuery
            (string resourceType, int skip, int take)
        {
            var expectedResults = Enumerable.Empty<BasicModel>();

            IQueryContext capturedQuery = null;
            var session = new Mock<IQuerySession>();
            session
                .Setup(x => x.Query<BasicModel>(It.IsAny<IQueryContext>()))
                .Callback<IQueryContext>(q => capturedQuery = q)
                .ReturnsAsync(expectedResults);

            var results = new TypeQueryable<BasicModel>(
                    session.Object,
                    new RemoteQueryProvider(session.Object, new JsonSerializerSettings()))
                .OrderBy(x => x.PropA)
                .Take(take)
                .Skip(skip)
                .ToArray();

            Assert.NotNull(results);
            Assert.Empty(results);

			Assert.NotNull(capturedQuery);
			Assert.NotEmpty(capturedQuery.Sort);
			Assert.NotNull(capturedQuery.PageOffset);
			Assert.NotNull(capturedQuery.PageLimit);

			Assert.Equal("propA", capturedQuery.Sort);
			Assert.Equal(skip, capturedQuery.PageOffset);
			Assert.Equal(take, capturedQuery.PageLimit);
        }

        private static SkipQueryable<TModel> CreateSubject<TModel>(RemoteQueryable<TModel> target, Expression skip)
        {
            return new SkipQueryable<TModel>(
                Mock.Of<IQuerySession>(),
                target,
                skip);
        }
    }
}