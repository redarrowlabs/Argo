using System.Collections.Generic;

namespace RedArrow.Argo.Client.Query
{
    public interface IQueryContext
    {
        string Sort { get; }

        int? PageSize { get; set; }
        int? PageNumber { get; set; }
        int? PageOffset { get; set; }
        int? PageLimit { get; set; }

        IDictionary<string, string> Filters { get; }
        void AppendSort(string sort, bool desc);
        void AppendFilter(string resourceType, string filter);
    }
}