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

		public FieldInfo SessionField { get; }
		public FieldInfo IncludeField { get; }

        public PropertyInfo SessionManagedProperty { get; }
        public PropertyInfo ResourceProperty { get; }
        public PropertyInfo PatchProperty { get; }

        public PropertyInfo IdProperty { get; }
		
        // resource attribute name => model property
        public IDictionary<string, AttributeConfiguration> AttributeConfigs { get; }

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
            PatchProperty = modelType.GetModelPatchProperty();

            IdProperty = modelType.GetModelIdProperty();

            AttributeConfigs = modelType.GetModelAttributeConfigurations();

            HasOneProperties = modelType.GetModelHasOneConfigurations();
            HasManyProperties = modelType.GetModelHasManyConfigurations();
        }
    }
}