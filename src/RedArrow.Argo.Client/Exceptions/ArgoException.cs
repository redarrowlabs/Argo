using System;

namespace RedArrow.Argo.Client.Exceptions
{
    public class ArgoException : Exception
    {
        protected Guid? Id { get; }
        protected Type Type { get; }

        public ArgoException(Type type, Guid? id = null)
        {
            Id = id;
            Type = type;
        }

        public ArgoException(string message, Type type, Guid? id = null) : base(message)
        {
            Id = id;
            Type = type;
        }

        public ArgoException(string message, Exception innerException, Type type, Guid? id = null) : base(message, innerException)
        {
            Id = id;
            Type = type;
        }

        public override string Message => $"\tModel Id:\t{Id}\n\tModel Type:\t{Type.FullName}\n{base.Message}";
    }
}