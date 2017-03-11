using System.Text;

namespace RedArrow.Argo.Client.Query
{
    public class QueryContext
    {
        public string Sort => SortBuilder.ToString();
        public string Filter => FilterBuilder.ToString();

        public StringBuilder SortBuilder { get; } = new StringBuilder();
        public StringBuilder FilterBuilder { get; } = new StringBuilder();

        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
    }
}
