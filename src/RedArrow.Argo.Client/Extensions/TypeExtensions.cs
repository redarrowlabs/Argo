using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Config.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RedArrow.Argo.Client.Extensions
{
    internal static class TypeExtensions
    {
        public static string GetModelResourceType(this Type type)
        {
            return type.GetTypeInfo()
                       .CustomAttributes
                       .Single(a => a.AttributeType == typeof(ModelAttribute))
                       .ConstructorArguments
                       .Select(arg => arg.Value as string)
                       .FirstOrDefault() ?? type.Name.Camelize();
        }

        public static MethodInfo GetInitializeMethod(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredMethods
                .Single(method => method.Name == "__argo__generated_Initialize");
        }

        public static FieldInfo GetSessionField(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredFields
                .Single(field => field.Name == "__argo__generated_session");
        }

        public static FieldInfo GetIncludeField(this Type type)
        {
            return type.GetTypeInfo()
                .DeclaredFields
                .Single(field => field.Name == "__argo__generated_include");
        }

        public static PropertyInfo GetSessionManagedProperty(this Type type)
        {
            return type.GetTypeInfo()
                .GetProperties()
                .Single(prop => prop.Name == "__argo__generated_SessionManaged");
        }

        public static PropertyInfo GetModelResourceProperty(this Type type)
        {
            return type.GetTypeInfo()
                .GetProperties()
                .Single(prop => prop.Name == "__argo__generated_Resource");
        }

        public static PropertyInfo GetModelIdProperty(this Type type)
        {
            return type.GetTypeInfo()
                .GetProperties()
                .Single(prop => prop.IsDefined(typeof(IdAttribute)));
        }

        public static IDictionary<string, AttributeConfiguration> GetModelAttributeConfigurations(this Type type)
        {
            return type.GetTypeInfo()
                .GetProperties()
                .Where(prop => prop.IsDefined(typeof(PropertyAttribute)))
                .Select(prop => new AttributeConfiguration(prop))
                .ToDictionary(
                    attrConfig => attrConfig.AttributeName,
                    attrConfig => attrConfig);
        }

        public static IDictionary<string, MetaConfiguration> GetModelMetaConfigurations(this Type type)
        {
            return type.GetTypeInfo()
                .GetProperties()
                .Where(prop => prop.IsDefined(typeof(MetaAttribute)))
                .Select(prop => new MetaConfiguration(prop))
                .ToDictionary(
                    metaConfig => metaConfig.MetaName,
                    metaConfig => metaConfig);
        }

        public static IDictionary<string, HasOneConfiguration> GetModelHasOneConfigurations(this Type type)
        {
            var fields = type
                .GetTypeInfo()
                .DeclaredFields
                .ToArray();
            return type
                .GetTypeInfo()
                .GetProperties()
                .Where(prop => prop.IsDefined(typeof(HasOneAttribute)))
                .Select(prop =>
                {
                    var isInitialized = fields
                        .Single(x => x.Name == $"__argo__generated_<{prop.Name}>k__BackingFieldInitialized");
                    return new HasOneConfiguration(prop, isInitialized);
                })
                .ToDictionary(
                    has1Cfg => has1Cfg.RelationshipName,
                    has1Cfg => has1Cfg);
        }

        public static IDictionary<string, HasManyConfiguration> GetModelHasManyConfigurations(this Type type)
        {
            return type.GetTypeInfo()
                .GetProperties()
                .Where(prop => prop.IsDefined(typeof(HasManyAttribute)))
                .Select(prop => new HasManyConfiguration(prop))
                .ToDictionary(
                    hasMCfg => hasMCfg.RelationshipName,
                    hasMCfg => hasMCfg);
        }
        
        private static IEnumerable<PropertyInfo> GetProperties(this TypeInfo typeInfo)
        {
            var properties = typeInfo.DeclaredProperties.ToList();

            if (typeInfo.BaseType != typeof(object))
            {
                properties.AddRange(typeInfo.BaseType.GetTypeInfo().GetProperties());
            }

            return properties;
        }
    }
}