using System;

namespace RedArrow.Jsorm.Core.Infrastructure
{
    public class JsormException : Exception
    {
        public JsormException()
        {
        }

        public JsormException(string message) : base(message)
        {
        }

        public JsormException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}