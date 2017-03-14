using System.Linq;
using Moq;
using RedArrow.Argo.Client.Session;
using RedArrow.Argo.Linq.Queryables;
using WovenByFody;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Linq.Queryable
{
    public class TypeQueryableTests
    {
        [Fact]
        public void BuildQuery__Then_ReturnNewQueryContext()
        {
            var subject = CreateSubject<BasicModel>(Mock.Of<IQuerySession>());

            var qc = subject.BuildQuery();

            Assert.NotNull(qc);
            Assert.Null(qc.PageSize);
            Assert.Null(qc.PageNumber);
            Assert.Empty(qc.Sort);
            Assert.Empty(qc.Filters);
        }

        private static TypeQueryable<TModel> CreateSubject<TModel>(IQuerySession session)
        {
            return new TypeQueryable<TModel>(session, Mock.Of<IQueryProvider>());
        }
    }
}
