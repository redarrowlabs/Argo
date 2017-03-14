using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Query;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Query
{
    public class QueryContextTests
    {
        [Theory, AutoData]
        public void AppendSort__Given_Sort__When_Ascending__Then_BuildCsv
            (string sort)
        {
            var subject = new QueryContext();

            subject.AppendSort(sort, false);

            var result = subject.Sort;

            Assert.Equal(sort, result);
        }

        [Theory, AutoData]
        public void AppendSort__Given_Sort__When_Descending__Then_BuildCsv
            (string sort)
        {
            var subject = new QueryContext();

            subject.AppendSort(sort, true);

            var result = subject.Sort;

            Assert.Equal($"-{sort}", result);
        }

        [Theory, AutoData]
        public void AppendSort__Given_MultiSort__When_AscendingDescendingMixed__Then_BuildCsv
            (string sortA, string sortB, string sortC)
        {
            var subject = new QueryContext();

            subject.AppendSort(sortA, true);
            subject.AppendSort(sortB, false);
            subject.AppendSort(sortC, true);

            var result = subject.Sort;

            Assert.Equal($"-{sortA},{sortB},-{sortC}", result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        public void AppendSort__Given_NullOrWhitespaceSort__Then_Return(string sort)
        {
            var subject = new QueryContext();

            subject.AppendSort(sort, true);

            var result = subject.Sort;

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory, AutoData]
        public void AppendFilter__Given_SingleType__Then_AppendFilter
            (string resourceType, string filterA, string filterB)
        {
            var subject = new QueryContext();

            subject.AppendFilter(resourceType, filterA);
            subject.AppendFilter(resourceType, filterB);

            var result = subject.Filters;

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(1, result.Count);
            Assert.True(result.ContainsKey(resourceType));
            Assert.Equal($"{filterA},{filterB}", result[resourceType]);
        }

        [Theory, AutoData]
        public void AppendFilter__Given_MultiType__Then_AppendFilter
            (string resourceTypeA, string resourceTypeB, string filterA, string filterB)
        {
            var subject = new QueryContext();

            subject.AppendFilter(resourceTypeA, filterA);
            subject.AppendFilter(resourceTypeA, filterB);
            
            subject.AppendFilter(resourceTypeB, filterB);
            subject.AppendFilter(resourceTypeB, filterA);

            var result = subject.Filters;

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);

            Assert.True(result.ContainsKey(resourceTypeA));
            Assert.True(result.ContainsKey(resourceTypeB));

            Assert.Equal($"{filterA},{filterB}", result[resourceTypeA]);
            Assert.Equal($"{filterB},{filterA}", result[resourceTypeB]);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        public void AppendFitler__Given_NullOrWhitespaceFilter__Then_Return(string filter)
        {
            var subject = new QueryContext();

            subject.AppendFilter("test", filter);

            var result = subject.Sort;

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        public void AppendFitler__Given_NullOrWhitespaceResourceType__Then_Return(string resourceType)
        {
            var subject = new QueryContext();

            subject.AppendFilter(resourceType, "filter");

            var result = subject.Sort;

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
