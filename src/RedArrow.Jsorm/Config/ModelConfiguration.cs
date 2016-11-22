using RedArrow.Jsorm.Attributes;
using RedArrow.Jsorm.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RedArrow.Jsorm.Config
{
    public class ModelConfiguration
    {
        internal ISet<Assembly> ScanAssemblies { get; }
        internal ISet<Type> ModelTypes { get; }

        internal ModelConfiguration()
        {
            ScanAssemblies = new HashSet<Assembly>();
            ModelTypes = new HashSet<Type>();
        }

        internal void Configure(SessionConfiguration config)
        {
            var modelsToConfigure = ModelTypes.Concat(ScanAssemblies.SelectMany(x => x.ExportedTypes))
                .Where(IsJsormModel)
                .ToArray();

            config.Types = modelsToConfigure.ToDictionary(
                x => x,
                x => (x.GetTypeInfo().CustomAttributes
                        .Single(a => a.AttributeType == typeof(ModelAttribute))
                        .ConstructorArguments
                        .Select(arg => arg.Value)
                        .FirstOrDefault() as string ?? x.Name)
                        .Camelize());

            config.IdProperties = modelsToConfigure
                .Select(x => x.GetTypeInfo()
                    .DeclaredProperties.Single(p => p.IsDefined(typeof(IdAttribute))))
                .ToArray();

            config.AttributeProperties = modelsToConfigure
                .SelectMany(x => x.GetTypeInfo()
                    .DeclaredProperties.Where(p => p.IsDefined(typeof(PropertyAttribute))))
                .Select(x => new PropertyConfiguration(x))
                .ToArray();

            config.HasOneProperties = modelsToConfigure
                .SelectMany(x => x.GetTypeInfo()
                    .DeclaredProperties.Where(p => p.IsDefined(typeof(HasOneAttribute))))
                .Select(x => new HasOneConfiguration(x))
                .ToArray();
        }

        private static bool IsJsormModel(Type modelType)
        {
            var typeInfo = modelType.GetTypeInfo();
            return typeInfo.IsClass
                   && (typeInfo.IsPublic
                       || (typeInfo.IsNested
                           && typeInfo.IsNestedPublic
                           && typeInfo.DeclaringType.GetTypeInfo().IsPublic))
                   && !typeInfo.IsAbstract
                   && typeInfo.IsDefined(typeof(ModelAttribute));
        }

        private string Camelize(string source)
        {
            return source.Substring(0, 1).ToLower() + source.Substring(1);
        }

        public ModelConfiguration AddFromAssemblyOf<T>()
        {
            ScanAssemblies.Add(typeof(T).GetTypeInfo().Assembly);
            return this;
        }

        public ModelConfiguration AddFromAssembly(Assembly assembly)
        {
            ScanAssemblies.Add(assembly);
            return this;
        }

        public ModelConfiguration Add<T>()
        {
            return Add(typeof(T));
        }

        public ModelConfiguration Add(Type type)
        {
            ModelTypes.Add(type);
            return this;
        }
    }
}