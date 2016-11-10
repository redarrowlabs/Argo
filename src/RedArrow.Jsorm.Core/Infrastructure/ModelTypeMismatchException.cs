using System;

namespace RedArrow.Jsorm.Core.Infrastructure
{
    public class ModelTypeMismatchException : JsormException
    {
        public ModelTypeMismatchException(Type requestedType, Type modelType)
            : base($"The requested model type {requestedType.FullName} is not assignable from the type cached with the requeted id: ", modelType)
        {
        }
    }
}