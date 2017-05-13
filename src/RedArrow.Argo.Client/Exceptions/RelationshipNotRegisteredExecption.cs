using System;

namespace RedArrow.Argo.Client.Exceptions
{
    public class RelationshipNotRegisteredExecption : ArgoException
    {
        public RelationshipNotRegisteredExecption(string rltnName, Type modelType)
            : base($"[HasMany] configuration named {rltnName} not found", modelType)
        {
        }
    }
}