using System;

namespace RedArrow.Argo.Client.Exceptions
{
    public class AttributeNotRegisteredException : ArgoException
    {
        public AttributeNotRegisteredException(string attrName, Type modelType)
            : base($"[Property] configuration named {attrName} not found", modelType)
        {
        }
    }
}
