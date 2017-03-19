using System;
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
    public class WhereQueryableTests
    {
        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleStringEquals__Then_AddFilter
            (string expectedValue)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x => x.StringProperty == expectedValue;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x => x.AppendFilter("allPropertyTypes", $"stringProperty[eq]'{expectedValue}'"), Times.Once);
        }
        
        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleGuidEquals__Then_AddFilter
            (Guid expectedValue)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x => x.GuidProperty == expectedValue;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x => x.AppendFilter("allPropertyTypes", $"guidProperty[eq]'{expectedValue}'"), Times.Once);
        }

        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleDateTimeEquals__Then_AddFilter
            (DateTime expectedValue)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x => x.DateTimeProperty == expectedValue;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x => x.AppendFilter("allPropertyTypes", $"dateTimeProperty[eq]'{expectedValue:O}'"), Times.Once);
        }

        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleIntEquals__Then_AddFilter
            (int expectedValue)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x => x.IntProperty == expectedValue;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x => x.AppendFilter("allPropertyTypes", $"intProperty[eq]{expectedValue}"), Times.Once);
        }

        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleLongEquals__Then_AddFilter
            (long expectedValue)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x => x.LongProperty == expectedValue;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x => x.AppendFilter("allPropertyTypes", $"longProperty[eq]{expectedValue}"), Times.Once);
        }

        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleDoubleEquals__Then_AddFilter
            (double expectedValue)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x => x.DoubleProperty == expectedValue;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x => x.AppendFilter("allPropertyTypes", $"doubleProperty[eq]{expectedValue}"), Times.Once);
        }

        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleDecimalEquals__Then_AddFilter
            (decimal expectedValue)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x => x.DecimalProperty == expectedValue;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x => x.AppendFilter("allPropertyTypes", $"decimalProperty[eq]{expectedValue}"), Times.Once);
        }

        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleFloatEquals__Then_AddFilter
            (float expectedValue)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x => x.FloatProperty == expectedValue;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x => x.AppendFilter("allPropertyTypes", $"floatProperty[eq]{expectedValue}"), Times.Once);
        }

        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleIntLessThan__Then_AddFilter
            (int expectedValue)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x => x.IntProperty < expectedValue;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x => x.AppendFilter("allPropertyTypes", $"intProperty[lt]{expectedValue}"), Times.Once);
        }

        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleIntGreaterThan__Then_AddFilter
            (int expectedValue)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x => x.IntProperty > expectedValue;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x => x.AppendFilter("allPropertyTypes", $"intProperty[gt]{expectedValue}"), Times.Once);
        }

        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleIntLessThanOrEqual__Then_AddFilter
            (int expectedValue)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x => x.IntProperty <= expectedValue;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x => x.AppendFilter("allPropertyTypes", $"intProperty[lte]{expectedValue}"), Times.Once);
        }

        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleIntGreaterThanOrEqual__Then_AddFilter
            (int expectedValue)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x => x.IntProperty >= expectedValue;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x => x.AppendFilter("allPropertyTypes", $"intProperty[gte]{expectedValue}"), Times.Once);
        }

        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleIntNotEqual__Then_AddFilter
            (int expectedValue)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x => x.IntProperty != expectedValue;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x => x.AppendFilter("allPropertyTypes", $"intProperty[ne]{expectedValue}"), Times.Once);
        }

        [Theory, AutoData]
        public void BuildQuery__Given_Target__When_ExpressionSimpleIntEqualsWithAndOR__Then_AddFilter
            (int expectedMin, int expectedMax, int expectedSpecific)
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<AllPropertyTypes>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<AllPropertyTypes, bool>> predicate = x =>
                x.IntProperty > expectedMin &&
                x.IntProperty < expectedMax ||
                x.IntProperty == expectedSpecific;

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            var result = subject.BuildQuery();

            Assert.Same(mockQueryContext.Object, result);

            mockQueryContext.Verify(x =>
                x.AppendFilter("allPropertyTypes", $"intProperty[gt]{expectedMin},intProperty[lt]{expectedMax},|intProperty[eq]{expectedSpecific}"),
                Times.Once);
        }

        private static WhereQueryable<TModel> CreateSubject<TModel>(
            IQuerySession session,
            RemoteQueryable<TModel> target,
            Expression<Func<TModel, bool>> predicate)
        {
            return new WhereQueryable<TModel>(
                session,
                target,
                predicate);
        }
    }
}
