using System;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using RedArrow.Argo.Client.Linq;
using RedArrow.Argo.Client.Linq.Queryables;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;
using WovenByFody;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Linq.Queryable
{
    public class OrderByQueryableTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BuildQuery__Given_TypeTarget__When_IsAttribute__Then_AddAttributeSort
            (bool expectedDesc)
        {
            var expectedSort = "propA";
            if (expectedDesc)
            {
                expectedSort = expectedSort.Insert(0, "-");
            }

            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<BasicModel>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            var subject = CreateSubject(
                mockTarget.Object,
                x => x.PropA,
                session,
                expectedDesc
            );

            var query = subject.BuildQuery();

            Assert.NotNull(query);

            Assert.Same(mockQueryContext.Object, query);

            mockQueryContext.Verify(x => x.AppendSort(expectedSort), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BuildQuery__Given_TypeTarget__When_IsMeta__Then_AddMetaMemberSort
            (bool expectedDesc)
        {
            var expectedSort = "meta.whatever";
            if (expectedDesc)
            {
                expectedSort = expectedSort.Insert(0, "-");
            }

            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<Widget>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            var subject = CreateSubject(
                mockTarget.Object,
                x => x.Whatever,
                session,
                expectedDesc
            );

            var query = subject.BuildQuery();

            Assert.NotNull(query);

            Assert.Same(mockQueryContext.Object, query);

            mockQueryContext.Verify(x => x.AppendSort(expectedSort), Times.Once);
        }

        [Fact]
        public void BuildQuery__When_ExpressionNotMemberExpression__Then_ThrowNotSupported()
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<BasicModel>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            var subject = CreateSubject(
                mockTarget.Object,
                x => x.GetHashCode(),
                session,
                true
            );

            Assert.Throws<NotSupportedException>(() => subject.BuildQuery());
        }

        private static OrderByQueryable<TModel, TComparable> CreateSubject<TModel, TComparable>(
            RemoteQueryable<TModel> target,
            Expression<Func<TModel, TComparable>> expression,
            IQuerySession session,
            bool isDesc)
        {
            return new OrderByQueryable<TModel, TComparable>(session, target, expression, isDesc);
        }
    }
}