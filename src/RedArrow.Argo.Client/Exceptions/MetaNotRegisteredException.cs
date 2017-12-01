using System;

namespace RedArrow.Argo.Client.Exceptions
{
    public class MetaNotRegisteredException : ArgoException
    {
        public MetaNotRegisteredException(string attrName, Type modelType)
            : base($"[Meta] configuration named {attrName} not found", modelType)
        {
        }
    }
}