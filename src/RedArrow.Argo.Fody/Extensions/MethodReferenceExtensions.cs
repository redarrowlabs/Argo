using System;
using Mono.Cecil;

namespace RedArrow.Argo.Extensions
{
	public static class MethodReferenceExtensions
	{
		public static MethodReference MakeGenericMethod(this MethodReference methRef, params TypeReference[] types)
		{
			if(methRef.GenericParameters.Count != types.Length)
				throw new ArgumentException();

			var ret = new GenericInstanceMethod(methRef);
			foreach (var typeRef in types)
			{
				ret.GenericArguments.Add(typeRef);
			}
			return ret;
		}
	}
}
