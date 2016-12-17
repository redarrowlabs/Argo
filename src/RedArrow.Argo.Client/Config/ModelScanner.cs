using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RedArrow.Argo.Attributes;
using RedArrow.Argo.Client.Config.Model;
using RedArrow.Argo.Client.Extensions;

namespace RedArrow.Argo.Client.Config
{
    public class ModelScanner
    {
        public ISet<Assembly> ScanAssemblies { get; }

        internal ModelScanner()
        {
            ScanAssemblies = new HashSet<Assembly>();
        }

        internal void Configure(SessionFactoryConfiguration config)
        {
            ScanAssemblies.SelectMany(x => x.ExportedTypes)
                .Where(IsJsormModel)
                .Select(x => new ModelConfiguration(x))
                .Each(config.Register);
        }

        public ModelScanner AssemblyOf<T>()
        {
            ScanAssemblies.Add(typeof(T).GetTypeInfo().Assembly);
            return this;
        }

        public ModelScanner Assembly(Assembly assembly)
        {
            ScanAssemblies.Add(assembly);
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