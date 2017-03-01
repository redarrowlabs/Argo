using System;

namespace RedArrow.Argo.Client.Exceptions
{
    public class ModelNotRegisteredException : ArgoException
    {
        public ModelNotRegisteredException(Type modelType)
            : base("No model registered for type:", modelType)
        {
        }
    }
}