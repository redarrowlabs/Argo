using System.Collections.Generic;

namespace RedArrow.Argo.Client.Query
{
    public interface IQueryContext
    {
        string BasePath { get; }

        int? PageSize { get; set; }
        int? PageNumber { get; set; }
        int? PageOffset { get; set; }
        int? PageLimit { get; set; }

        string Sort { get; }
        IDictionary<string, string> Filters { get; }

        void AppendSort(string sort);

        void AppendFilter(string resourceType, string filter);

        string BuildPath();
    }
}