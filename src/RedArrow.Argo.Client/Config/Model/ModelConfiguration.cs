using System;
using System.Collections.Generic;
using System.Reflection;
using RedArrow.Argo.Client.Extensions;

namespace RedArrow.Argo.Client.Config.Model
{
    public class ModelConfiguration
    {
        public Type ModelType { get; }
        public string ResourceType { get; }

        public PropertyInfo IdProperty { get; }

        // resource attribute name => model property
        public IDictionary<string, AttributeConfiguration> AttributeProperties { get; }

        // resource relationship name => model property
        public IDictionary<string, HasOneConfiguration> HasOneProperties { get; }

        internal ModelConfiguration(Type modelType)
        {
            ModelType = modelType;
            ResourceType = modelType.GetModelResourceType();
            IdProperty = modelType.GetModelIdProperty();
            AttributeProperties = modelType.GetModelAttributeConfigurations();
            HasOneProperties = modelType.GetModelHasOneConfigurations();
        }
    }
}