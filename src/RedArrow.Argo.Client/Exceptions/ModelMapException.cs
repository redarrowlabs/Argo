using System;

namespace RedArrow.Argo.Client.Exceptions
{
    public class ModelMapException : ArgoException
    {
        public ModelMapException(string message, Type modelType, Guid id) : base(message, modelType, id)
        {
        }
    }
}
