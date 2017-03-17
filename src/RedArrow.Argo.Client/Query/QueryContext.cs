using System.Collections.Generic;
using System.Linq;

namespace RedArrow.Argo.Client.Query
{
    public class QueryContext : IQueryContext
    {
        public string Sort => string.Join(",", SortBuilder);
        private ICollection<string> SortBuilder { get; } = new List<string>();

        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
        public int? PageOffset { get; set; }
        public int? PageLimit { get; set; }

        public IDictionary<string, string> Filters => FilterBuilders.ToDictionary(
            x => x.Key,
            x => string.Join(",", x.Value));
        private IDictionary<string, ICollection<string>> FilterBuilders { get; } = new Dictionary<string, ICollection<string>>();

        public void AppendSort(string sort, bool desc)
        {
            if (string.IsNullOrWhiteSpace(sort)) return;

            if (desc)
            {
                sort = sort.Insert(0, "-");
            }
            SortBuilder.Add(sort);
        }

        public void AppendFilter(string resourceType, string filter)
        {
            if (string.IsNullOrWhiteSpace(resourceType) || string.IsNullOrWhiteSpace(filter)) return;

            ICollection<string> builder;
            if (!FilterBuilders.TryGetValue(resourceType, out builder))
            {
                builder = new List<string>();
                FilterBuilders[resourceType] = builder;
            }

            builder.Add(filter);
        }
    }
}
