using System;

namespace RedArrow.Jsorm.Infrastructure
{
    public class ModelNotRegisteredException : JsormException
    {
        public ModelNotRegisteredException(Type modelType)
            : base("No model registered for type:", modelType)
        {
        }
    }
}