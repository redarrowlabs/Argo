using System;

namespace RedArrow.Argo.Client.Exceptions
{
	public class UnmanagedModelException : ArgoException
	{
		public UnmanagedModelException(Guid modelId, Type modelType) :
			base($"model {{{modelId}}} is not managed by the current session", modelType)
		{
		}
	}
}
