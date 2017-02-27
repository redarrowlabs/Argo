using System;

namespace RedArrow.Argo.Client.Exceptions
{
	public class ManagedModelCreationException : ArgoException
	{
		public ManagedModelCreationException(Guid modelId, Type modelType) :
			base($"Attempt was made to create managed model {{{modelId}}}.  This model has already been assigned an Id and persisted.", modelType)
		{
		}
	}
}
