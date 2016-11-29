using RedArrow.Jsorm.Attributes;
using RedArrow.Jsorm.Config.Model;
using RedArrow.Jsorm.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RedArrow.Jsorm.Config
{
    public class ModelLocator
    {
        public ISet<Assembly> ScanAssemblies { get; }
        public ISet<Type> ModelTypes { get; }

        internal ModelLocator()
        {
            ScanAssemblies = new HashSet<Assembly>();
            ModelTypes = new HashSet<Type>();
        }

        internal void Configure(SessionFactoryConfiguration config)
        {
            ModelTypes.Concat(ScanAssemblies.SelectMany(x => x.ExportedTypes))
                .Where(IsJsormModel)
                .Select(x => new ModelConfiguration(x))
                .Each(config.Register);
        }

        public ModelLocator AddFromAssemblyOf<T>()
        {
            ScanAssemblies.Add(typeof(T).GetTypeInfo().Assembly);
            return this;
        }

        public ModelLocator AddFromAssembly(Assembly assembly)
        {
            ScanAssemblies.Add(assembly);
            return this;
        }

        public ModelLocator Add<T>()
        {
            return Add(typeof(T));
        }

        public ModelLocator Add(Type type)
        {
            ModelTypes.Add(type);
            return this;
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
    }
}