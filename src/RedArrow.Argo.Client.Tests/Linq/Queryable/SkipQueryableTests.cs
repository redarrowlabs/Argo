using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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
    public class SkipQueryableTests
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

            Assert.NotNull(result.PageNumber);
            Assert.Equal(2, result.PageNumber.Value);

            Assert.NotNull(result.PageSize);
            Assert.Equal(skip, result.PageSize.Value);
        }

        private SkipQueryable<TModel> CreateSubject<TModel>(RemoteQueryable<TModel> target, Expression skip)
        {
            return new SkipQueryable<TModel>(
                Mock.Of<IQuerySession>(),
                target,
                skip);
        }
    }
}
