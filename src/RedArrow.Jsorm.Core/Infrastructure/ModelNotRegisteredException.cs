using System;

namespace RedArrow.Jsorm.Core.Infrastructure
{
    public class ModelNotRegisteredException : JsormException
    {
        public ModelNotRegisteredException(Type modelType)
            : base("No model registered for type:", modelType)
        {
        }
    }
}