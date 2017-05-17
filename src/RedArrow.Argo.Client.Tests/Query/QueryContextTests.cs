using Ploeh.AutoFixture.Xunit2;
using RedArrow.Argo.Client.Query;
using WovenByFody;
using Xunit;

namespace RedArrow.Argo.Client.Tests.Query
{
    public class QueryContextTests
    {
        [Fact]
        public void Ctor__Given_ModelType__Then_BasPathEqualsResourceType()
        {
            var subject = new QueryContext<BasicModel>();

            Assert.Equal("basicModel", subject.BasePath);
        }

        [Theory, AutoData]
        public void AppendSort__Given_Sort__Then_BuildCsv
            (string sort)
        {
            var subject = new QueryContext<BasicModel>();

            subject.AppendAttributesSort(sort);

            var result = subject.AttributesSort;

            Assert.Equal(sort, result);
        }

        [Theory, AutoData]
        public void AppendSort__Given_MultiSort__When_Multiple__Then_BuildCsv
            (string sortA, string sortB, string sortC)
        {
            var subject = new QueryContext<BasicModel>();

            subject.AppendAttributesSort(sortA);
            subject.AppendAttributesSort(sortB);
            subject.AppendAttributesSort(sortC);

            var result = subject.AttributesSort;

            Assert.Equal($"{sortA},{sortB},{sortC}", result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        public void AppendSort__Given_NullOrWhitespaceSort__Then_Return
            (string sort)
        {
            var subject = new QueryContext<BasicModel>();

            subject.AppendAttributesSort(sort);

            var result = subject.AttributesSort;

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory, AutoData]
        public void AppendAttributesFilter__Given_SingleType__Then_AppendAttributesFilter
            (string resourceType, string filterA, string filterB)
        {
            var subject = new QueryContext<BasicModel>();

            subject.AppendAttributesFilter(resourceType, filterA);
            subject.AppendAttributesFilter(resourceType, filterB);

            var result = subject.AttributesFilters;

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(1, result.Count);
            Assert.True(result.ContainsKey(resourceType));
            Assert.Equal($"{filterA},{filterB}", result[resourceType]);
        }

        [Theory, AutoData]
        public void AppendAttributesFilter__Given_MultiType__Then_AppendAttributesFilter
            (string resourceType, string resourceTypeA, string resourceTypeB, string filterA, string filterB)
        {
            var subject = new QueryContext<BasicModel>();

            subject.AppendAttributesFilter(resourceTypeA, filterA);
            subject.AppendAttributesFilter(resourceTypeA, filterB);
            
            subject.AppendAttributesFilter(resourceTypeB, filterB);
            subject.AppendAttributesFilter(resourceTypeB, filterA);

            var result = subject.AttributesFilters;

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);

            Assert.True(result.ContainsKey(resourceTypeA));
            Assert.True(result.ContainsKey(resourceTypeB));

            Assert.Equal($"{filterA},{filterB}", result[resourceTypeA]);
            Assert.Equal($"{filterB},{filterA}", result[resourceTypeB]);
        }

        [Theory]
        [InlineData("resourceType", null)]
        [InlineData("resourceType", "")]
        [InlineData("resourceType", "\t")]
        public void AppendFitler__Given_NullOrWhitespaceFilter__Then_Return
            (string resourceType, string filter)
        {
            var subject = new QueryContext<BasicModel>();

            subject.AppendAttributesFilter("test", filter);

            var result = subject.AttributesSort;

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\t")]
        public void AppendFitler__Given_NullOrWhitespaceResourceType__Then_Return
            (string resourceType)
        {
            var subject = new QueryContext<BasicModel>();

            subject.AppendAttributesFilter(resourceType, "filter");

            var result = subject.AttributesSort;

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}