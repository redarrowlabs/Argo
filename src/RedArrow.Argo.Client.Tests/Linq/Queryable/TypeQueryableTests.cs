using System.Linq;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Linq.Queryables;
using RedArrow.Argo.Client.Session;
using WovenByFody;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Linq.Queryable
{
    public class TypeQueryableTests
    {
        [Fact]
        public void BuildQuery__Then_ReturnNewQueryContext()
        {
            var subject = CreateSubject<BasicModel>();

            var qc = subject.BuildQuery();

            Assert.NotNull(qc);
            Assert.Null(qc.PageSize);
            Assert.Null(qc.PageNumber);
            Assert.Empty(qc.Sort);
            Assert.Empty(qc.Filters);
        }

        private static TypeQueryable<TModel> CreateSubject<TModel>()
        {
            return new TypeQueryable<TModel>(
				Mock.Of<IQuerySession>(),
				Mock.Of<IQueryProvider>());
        }
    }
}
