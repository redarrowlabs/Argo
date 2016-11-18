using Mono.Cecil;

namespace RedArrow.Jsorm
{
	public partial class ModuleWeaver
	{

		private GenericInstanceMethod SupplyGenericArgs(
			ModelWeavingContext context,
			PropertyDefinition propertyDef,
			MethodReference methodRef)
		{
			var genericMethod = new GenericInstanceMethod(methodRef);
			genericMethod.GenericArguments.Add(context.ModelTypeRef);
			genericMethod.GenericArguments.Add(propertyDef.GetMethod.ReturnType);
			return genericMethod;
		}
	}
}
