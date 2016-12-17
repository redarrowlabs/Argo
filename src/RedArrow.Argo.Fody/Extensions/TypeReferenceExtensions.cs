using System.Linq;
using Mono.Cecil;

namespace RedArrow.Argo.Extensions
{
	public static class TypeReferenceExtensions
	{
		public static MethodReference EqualityOperator(this TypeReference self)
		{
			return self.Resolve()
				?.Methods
				.Where(x => x.IsStatic)
				.Where(x => x.IsSpecialName)
				.Where(x => x.IsPublic)
				.SingleOrDefault(x => x.Name == "op_Equality");
		}
	}
}
