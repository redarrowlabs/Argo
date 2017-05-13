using System;

namespace RedArrow.Argo.Client.Exceptions
{
    public class UnmanagedModelException : ArgoException
    {
        public UnmanagedModelException(Type modelType, Guid id) :
            base($"model {{{id}}} is not managed by the current session", modelType, id)
        {
        }
    }
}