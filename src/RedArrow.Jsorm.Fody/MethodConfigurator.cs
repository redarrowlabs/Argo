using Mono.Cecil;

namespace RedArrow.Jsorm
{
	public partial class ModuleWeaver
	{
		private static GenericInstanceMethod SupplyGenericArgs(MethodReference methodRef, params TypeReference[] types)
		{
			var genericMethod = new GenericInstanceMethod(methodRef);
            
            foreach (var typeReference in types)
            {
                genericMethod.GenericArguments.Add(typeReference);
            }
			return genericMethod;
		}
	}
}
