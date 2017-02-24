using System;

namespace RedArrow.Argo.Client.Exceptions
{
    public class ArgoException : Exception
    {
        private Type Type { get; }

        public ArgoException(Type type)
        {
            Type = type;
        }

        public ArgoException(string message, Type type) : base(message)
        {
            Type = type;
        }

        public ArgoException(string message, Exception innerException, Type type) : base(message, innerException)
        {
            Type = type;
        }

        public override string Message => $"{base.Message}{Type.FullName}";
    }
}