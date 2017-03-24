namespace RedArrow.Argo.Client.Linq.Behaviors
{
	internal static class QueryBahavior
	{
		internal static IQueryBehavior ByType { get; } = new QueryByTypeBehavior();
		internal static IQueryBehavior ByRelationship { get; } = new QueryByRelationshipBehavior();
	}
}
