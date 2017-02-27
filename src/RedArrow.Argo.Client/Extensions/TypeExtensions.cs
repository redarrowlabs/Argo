using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Exceptions;
using RedArrow.Argo.Model;
using RedArrow.Argo.Session;

namespace RedArrow.Argo.Client.Extensions
{
    internal static class TypeExtensions
    {
        internal static ConstructorInfo GetDefaultConstructor(this Type type)
        {
            if (type == null || type.GetTypeInfo().IsAbstract)
            {
                return null;
            }

            var result = type.GetTypeInfo()
                .DeclaredConstructors
                .FirstOrDefault(ctor => ctor.IsPublic && !ctor.GetParameters().Any());

            if (result == null)
            {
                throw new ArgoException("A default (no-arg) constructor could not be found for: ", type);
            }

            return result;
        }

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
			    .Where(field => field.FieldType == typeof (IModelSession))
			    .Single(field => field.Name == "__argo__Generated_session");
	    }

        internal static PropertyInfo GetSessionManagedProperty(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredProperties
                .Where(prop => prop.PropertyType == typeof(bool))
                .Single(prop => prop.Name == "__argo__generated_SessionManaged");
        }

        internal static PropertyInfo GetModelResourceProperty(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredProperties
                .Where(prop => prop.PropertyType == typeof(IResourceIdentifier))
                .Single(prop => prop.Name == "__argo__generated_Resource");
        }

        internal static PropertyInfo GetModelPatchProperty(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredProperties
                .Where(prop => prop.PropertyType == typeof(IResourceIdentifier))
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

        internal static PropertyInfo GetPropertyBagProperty(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredProperties
                .SingleOrDefault(prop => prop.IsDefined(typeof(PropertyBagAttribute)));
        }
    }
}