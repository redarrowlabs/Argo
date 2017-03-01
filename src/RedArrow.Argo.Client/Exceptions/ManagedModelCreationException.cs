using System;

namespace RedArrow.Argo.Client.Exceptions
{
	public class ManagedModelCreationException : ArgoException
	{
		public ManagedModelCreationException(Type modelType, Guid id) :
			base($"Attempt was made to create managed model {{{id}}}.  This model has already been assigned an Id and persisted.", modelType, id)
		{
		}
	}
}
