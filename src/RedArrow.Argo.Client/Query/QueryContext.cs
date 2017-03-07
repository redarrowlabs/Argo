namespace RedArrow.Argo.Client.Query
{
    public class QueryContext
    {
        public string Sort { get; set; }
        public string Filter { get; set; }
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
    }
}
