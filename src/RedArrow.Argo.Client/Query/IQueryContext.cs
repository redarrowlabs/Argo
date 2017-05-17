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

        string AttributesSort { get; }
        IDictionary<string, string> AttributesFilters { get; }
        void AppendAttributesSort(string sort);
        void AppendAttributesFilter(string resourceType, string filter);

        string MetaSort { get; }
        string MetaFilters { get; }
        void AppendMetaSort(string sort);
        void AppendMetaFilter(string filter);

        string BuildPath();
    }
}