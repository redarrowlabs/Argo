using System;

namespace RedArrow.Jsorm.Client.Infrastructure
{
    public class ModelTypeMismatchException : JsormException
    {
        public ModelTypeMismatchException(Type requestedType, Type modelType)
            : base($"The requested model type {requestedType.FullName} is not assignable from the type cached with the requeted id: ", modelType)
        {
        }
    }
}