using RedArrow.Argo.Client.Extensions;
using RedArrow.Argo.Client.Flurl.Shared;
using System.Collections.Generic;
using System.Linq;

namespace RedArrow.Argo.Client.Query
{
    public class QueryContext<TModel> : IQueryContext
    {
        public string BasePath { get; protected set; }

        public string AttributesSort => string.Join(",", AttributesSortBuilder);
        private ICollection<string> AttributesSortBuilder { get; } = new List<string>();

        public string MetaSort => string.Join(",", MetaSortBuilder);
        private ICollection<string> MetaSortBuilder { get; } = new List<string>();

        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
        public int? PageOffset { get; set; }
        public int? PageLimit { get; set; }

        public IDictionary<string, string> AttributesFilters => AttributesFilterBuilders.ToDictionary(
            x => x.Key,
            x => string.Join(",", x.Value));

        private IDictionary<string, ICollection<string>> AttributesFilterBuilders { get; } = new Dictionary<string, ICollection<string>>();

        public string MetaFilters => string.Join(",", MetaFilterBuilders);

        private ICollection<string> MetaFilterBuilders { get; } = new List<string>();

        public QueryContext()
        {
            BasePath = typeof(TModel).GetModelResourceType();
        }

        public void AppendAttributesSort(string sort)
        {
            if (string.IsNullOrWhiteSpace(sort)) return;

            AttributesSortBuilder.Add(sort);
        }

        public void AppendMetaSort(string sort)
        {
            if (string.IsNullOrWhiteSpace(sort)) return;

            MetaSortBuilder.Add(sort);
        }

        public void AppendAttributesFilter(string resourceType, string filter)
        {
            if (string.IsNullOrWhiteSpace(resourceType) || string.IsNullOrWhiteSpace(filter)) return;

            ICollection<string> builder;
            if (!AttributesFilterBuilders.TryGetValue(resourceType, out builder))
            {
                builder = new List<string>();
                AttributesFilterBuilders[resourceType] = builder;
            }

            builder.Add(filter);
        }

        public void AppendMetaFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return;

            MetaFilterBuilders.Add(filter);
        }

        public string BuildPath()
        {
            var path = BasePath;
            if (AttributesFilters != null)
            {
                path = AttributesFilters.Aggregate(path, (current, kvp) => current.SetQueryParam($"filter[{kvp.Key}]", kvp.Value));
            }

            if (!string.IsNullOrEmpty(AttributesSort))
            {
                path = path.SetQueryParam("sort", AttributesSort);
            }

            if (!string.IsNullOrEmpty(MetaFilters))
            {
                path = path.SetQueryParam("meta[filter]", MetaFilters);
            }

            if (!string.IsNullOrEmpty(MetaSort))
            {
                path = path.SetQueryParam("meta[sort]", MetaSort);
            }

            if (PageSize != null)
            {
                path = path.SetQueryParam("page[size]", PageSize);
            }

            if (PageNumber != null)
            {
                path = path.SetQueryParam("page[number]", PageNumber);
            }

            if (PageOffset != null)
            {
                path = path.SetQueryParam("page[offset]", PageOffset);
            }

            if (PageLimit != null)
            {
                path = path.SetQueryParam("page[limit]", PageLimit);
            }

            return path;
        }
    }
}