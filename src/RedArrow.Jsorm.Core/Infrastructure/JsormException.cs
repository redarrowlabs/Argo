using System;

namespace RedArrow.Jsorm.Core.Infrastructure
{
    public class JsormException : Exception
    {
        private Type Type { get; }

        public JsormException(Type type)
        {
            Type = type;
        }

        public JsormException(string message, Type type) : base(message)
        {
            Type = type;
        }

        public JsormException(string message, Exception innerException, Type type) : base(message, innerException)
        {
            Type = type;
        }

        public override string Message => $"{base.Message}{Type.FullName}";
    }
}