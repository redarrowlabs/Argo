﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using RedArrow.Argo.Client.Flurl.Shared;

namespace RedArrow.Argo.Client.Query
{
    public class QueryContext : IQueryContext
    {
	    public string BasePath { get; }

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

	    public QueryContext(string basePath)
	    {
		    BasePath = basePath;
	    }

        public void AppendSort(string sort)
        {
            if (string.IsNullOrWhiteSpace(sort)) return;
            
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

	    public string BuildPath()
	    {
		    var path = BasePath;
		    if (Filters != null)
		    {
			    path = Filters.Aggregate(path, (current, kvp) => current.SetQueryParam($"filter[{kvp.Key}]", kvp.Value));
		    }

		    if (!string.IsNullOrEmpty(Sort))
		    {
			    path = path.SetQueryParam("sort", Sort);
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
