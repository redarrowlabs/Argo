using Moq;
using Newtonsoft.Json;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Linq;
using RedArrow.Argo.Client.Linq.Queryables;
using RedArrow.Argo.Client.Query;
using RedArrow.Argo.Client.Session;
using System;
using System.Linq;
using System.Linq.Expressions;
using WovenByFody;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Linq.Queryable
{
    public class WhereMetaQueryableTests
    {
        [Fact]
        public void BuildQuery__Given_Target__When_ExpressionHasProperty__Then_Throws()
        {
            var mockQueryContext = new Mock<IQueryContext>();

            var session = Mock.Of<IQuerySession>();

            var mockTarget = new Mock<RemoteQueryable<Widget>>(session, Mock.Of<IQueryProvider>());
            mockTarget
                .Setup(x => x.BuildQuery())
                .Returns(mockQueryContext.Object);

            Expression<Func<Widget, bool>> predicate = x => x.Whatever == "lalala" && x.Sku == "9876";

            var subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            Assert.Throws<NotSupportedException>(() => subject.BuildQuery());

            predicate = x => x.ETag == "abc123" && x.Name == "Thing";

            subject = CreateSubject(
                session,
                mockTarget.Object,
                predicate);

            Assert.Throws<NotSupportedException>(() => subject.BuildQuery());
        }

        private static WhereQueryable<TModel> CreateSubject<TModel>(
            IQuerySession session,
            RemoteQueryable<TModel> target,
            Expression<Func<TModel, bool>> predicate,
            JsonSerializerSettings jsonSettings = null)
        {
            return new WhereMetaQueryable<TModel>(
                session,
                target,
                predicate,
                jsonSettings ?? new JsonSerializerSettings());
        }
    }
}