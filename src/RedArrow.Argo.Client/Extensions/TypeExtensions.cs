using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Config.Model;

namespace RedArrow.Argo.Client.Extensions
{
    internal static class TypeExtensions
    {
        internal static string GetModelResourceType(this Type type)
        {
            return type.GetTypeInfo()
                .CustomAttributes
                .Single(a => a.AttributeType == typeof(ModelAttribute))
                .ConstructorArguments
                .Select(arg => arg.Value as string)
                .FirstOrDefault() ?? type.Name.Camelize();
        }

	    internal static FieldInfo GetSessionField(this Type type)
	    {
		    return type.GetTypeInfo()
			    .DeclaredFields
			    .Single(field => field.Name == "__argo__generated_session");
	    }

	    internal static FieldInfo GetIncludeField(this Type type)
	    {
		    return type.GetTypeInfo()
			    .DeclaredFields
			    .Single(field => field.Name == "__argo__generated_include");

	    }

        internal static PropertyInfo GetSessionManagedProperty(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredProperties
                .Single(prop => prop.Name == "__argo__generated_SessionManaged");
        }

        internal static PropertyInfo GetModelResourceProperty(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredProperties
                .Single(prop => prop.Name == "__argo__generated_Resource");
        }

        internal static PropertyInfo GetModelPatchProperty(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredProperties
                .Single(prop => prop.Name == "__argo__generated_Patch");
        }

        internal static PropertyInfo GetModelIdProperty(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredProperties
                .Single(prop => prop.IsDefined(typeof(IdAttribute)));
        }

        internal static IDictionary<string, AttributeConfiguration> GetModelAttributeConfigurations(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredProperties
                .Where(prop => prop.IsDefined(typeof(PropertyAttribute)))
                .Select(prop => new AttributeConfiguration(prop))
                .ToDictionary(
                    attrConfig => attrConfig.AttributeName,
                    attrConfig => attrConfig);
        }

        internal static IDictionary<string, RelationshipConfiguration> GetModelHasOneConfigurations(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredProperties
                .Where(prop => prop.IsDefined(typeof(HasOneAttribute)))
                .Select(prop => new HasOneConfiguration(prop))
				.Cast<RelationshipConfiguration>()
                .ToDictionary(
                    has1Cfg => has1Cfg.RelationshipName,
                    has1Cfg => has1Cfg);
        }

        internal static IDictionary<string, RelationshipConfiguration> GetModelHasManyConfigurations(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredProperties
                .Where(prop => prop.IsDefined(typeof(HasManyAttribute)))
                .Select(prop => new HasManyConfiguration(prop))
				.Cast<RelationshipConfiguration>()
				.ToDictionary(
                    hasMCfg => hasMCfg.RelationshipName,
                    hasMCfg => hasMCfg);
        }

        internal static PropertyInfo GetUnmappedAttributesProperty(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredProperties
                .SingleOrDefault(prop => prop.IsDefined(typeof(UnmappedAttribute)));
        }
    }
}