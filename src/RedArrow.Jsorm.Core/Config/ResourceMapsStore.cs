using RedArrow.Jsorm.Core.Map;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RedArrow.Jsorm.Core.Config
{
    public class ResourceMapsStore
    {
        private IList<Assembly> ScanAssemblies { get; }
        private IList<Type> MapTypes { get; }

        internal ResourceMapsStore()
        {
            ScanAssemblies = new List<Assembly>();
            MapTypes = new List<Type>();
        }

        internal void Configure(SessionConfiguration config)
        {
            foreach (var assembly in ScanAssemblies)
            {
                config.AddMapsFromAssembly(assembly);
            }

            foreach (var type in MapTypes)
            {
                config.Add(type);
            }
        }

        public ResourceMapsStore AddFromAssemblyOf<T>()
        {
            ScanAssemblies.Add(typeof(T).GetTypeInfo().Assembly);
            return this;
        }

        public ResourceMapsStore AddFromAssembly(Assembly assembly)
        {
            ScanAssemblies.Add(assembly);
            return this;
        }

        public ResourceMapsStore Add<T>()
            where T : IResourceMap
        {
            MapTypes.Add(typeof(T));
            return this;
        }
    }
}