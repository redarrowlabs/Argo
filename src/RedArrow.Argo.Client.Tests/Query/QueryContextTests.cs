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

            subject.AppendSort(sort);

            var result = subject.Sort;

            Assert.Equal(sort, result);
        }

        [Theory, AutoData]
        public void AppendSort__Given_MultiSort__When_Multiple__Then_BuildCsv
            (string sortA, string sortB, string sortC)
        {
            var subject = new QueryContext<BasicModel>();

            subject.AppendSort(sortA);
            subject.AppendSort(sortB);
            subject.AppendSort(sortC);

            var result = subject.Sort;

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

            subject.AppendSort(sort);

            var result = subject.Sort;

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory, AutoData]
        public void AppendFilter__Given_SingleType__Then_AppendFilter
            (string resourceType, string filterA, string filterB)
        {
            var subject = new QueryContext<BasicModel>();

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
            (string resourceType, string resourceTypeA, string resourceTypeB, string filterA, string filterB)
        {
            var subject = new QueryContext<BasicModel>();

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
        [InlineData("resourceType", null)]
        [InlineData("resourceType", "")]
        [InlineData("resourceType", "\t")]
        public void AppendFitler__Given_NullOrWhitespaceFilter__Then_Return
			(string resourceType, string filter)
        {
            var subject = new QueryContext<BasicModel>();

            subject.AppendFilter("test", filter);

            var result = subject.Sort;

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

            subject.AppendFilter(resourceType, "filter");

            var result = subject.Sort;

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
