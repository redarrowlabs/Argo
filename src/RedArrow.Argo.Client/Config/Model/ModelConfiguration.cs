using RedArrow.Argo.Client.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RedArrow.Argo.Client.Config.Model
{
    public class ModelConfiguration
    {
        public Type ModelType { get; }
        public string ResourceType { get; }

        public FieldInfo SessionField { get; }
        public FieldInfo IncludeField { get; }

        public PropertyInfo SessionManagedProperty { get; }
        public PropertyInfo ResourceProperty { get; }

        public PropertyInfo IdProperty { get; }

        // resource attribute name => model property
        public IDictionary<string, AttributeConfiguration> AttributeConfigs { get; }

        // resource meta name -> meta property
        public IDictionary<string, MetaConfiguration> MetaConfigs { get; }

        // resource relationship name => model property
        public IDictionary<string, RelationshipConfiguration> HasOneProperties { get; }

        // resource relationship name => model property
        public IDictionary<string, RelationshipConfiguration> HasManyProperties { get; }

        internal ModelConfiguration(Type modelType)
        {
            ModelType = modelType;
            ResourceType = modelType.GetModelResourceType();

            SessionField = modelType.GetSessionField();
            IncludeField = modelType.GetIncludeField();

            SessionManagedProperty = modelType.GetSessionManagedProperty();
            ResourceProperty = modelType.GetModelResourceProperty();

            IdProperty = modelType.GetModelIdProperty();

            AttributeConfigs = modelType.GetModelAttributeConfigurations();
            MetaConfigs = modelType.GetModelMetaConfigurations();

            HasOneProperties = modelType.GetModelHasOneConfigurations();
            HasManyProperties = modelType.GetModelHasManyConfigurations();
        }
    }
}