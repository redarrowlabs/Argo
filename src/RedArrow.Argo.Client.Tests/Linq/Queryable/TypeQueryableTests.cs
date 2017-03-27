using System.Linq;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Linq.Behaviors;
using RedArrow.Argo.Client.Linq.Queryables;
using RedArrow.Argo.Client.Session;
using WovenByFody;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Linq.Queryable
{
    public class TypeQueryableTests
    {
        [Theory, AutoData]
        public void BuildQuery__Then_ReturnNewQueryContext(string basePath)
        {
            var subject = CreateSubject<BasicModel>(basePath);

            var qc = subject.BuildQuery();

            Assert.NotNull(qc);
            Assert.Null(qc.PageSize);
            Assert.Null(qc.PageNumber);
            Assert.Empty(qc.Sort);
            Assert.Empty(qc.Filters);
        }

        private static TypeQueryable<TModel> CreateSubject<TModel>(string basePath)
        {
            return new TypeQueryable<TModel>(
				basePath,
				Mock.Of<IQuerySession>(),
				Mock.Of<IQueryProvider>(),
				Mock.Of<IQueryBehavior>());
        }
    }
}
