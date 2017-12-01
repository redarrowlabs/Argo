using System;

namespace RedArrow.Argo.Client.Exceptions
{
    public class ModelTypeMismatchException : ArgoException
    {
        public ModelTypeMismatchException(Type requestedType, Type modelType)
            : base($"The requested model type {requestedType.FullName} is not assignable from the type cached with the requeted id: ", modelType)
        {
        }
    }
}