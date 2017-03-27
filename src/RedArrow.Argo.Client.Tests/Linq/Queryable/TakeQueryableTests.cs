using System.Linq;
using System.Linq.Expressions;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Linq;
using RedArrow.Argo.Client.Linq.Queryables;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;
using WovenByFody;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Linq.Queryable
{
    public class TakeQueryableTests
    {
        [Theory, AutoData]
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

            mockQueryContext.VerifySet(x => x.PageLimit = skip, Times.Once);

            mockQueryContext.VerifySet(x => x.PageOffset = It.IsAny<int>(), Times.Never());
            mockQueryContext.VerifySet(x => x.PageSize = It.IsAny<int>(), Times.Never());
            mockQueryContext.VerifySet(x => x.PageNumber = It.IsAny<int>(), Times.Never());
        }

        private static TakeQueryable<TModel> CreateSubject<TModel>(RemoteQueryable<TModel> target, Expression skip)
        {
            return new TakeQueryable<TModel>(
                Mock.Of<IQuerySession>(),
                target,
                skip);
        }
    }
}
