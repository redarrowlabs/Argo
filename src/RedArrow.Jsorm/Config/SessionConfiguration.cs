using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RedArrow.Jsorm.Extensions;
using RedArrow.Jsorm.Map;
using RedArrow.Jsorm.Registry;
using RedArrow.Jsorm.Session;

namespace RedArrow.Jsorm.Config
{
    public class SessionConfiguration
    {
        internal IList<IResourceMap> Maps { get; }
        internal ICacheProvider CacheProvider { get; set; }

        internal SessionConfiguration()
        {
            Maps = new List<IResourceMap>();
            CacheProvider = new DefaultCacheProvider();
        }

        public ISessionFactory BuildSessionFactory()
        {
            var factory = new SessionFactory(CacheProvider);

            foreach (var map in Maps)
            {
                map.Configure(factory);
            }

            return factory;
        }

        public void AddMapsFromAssembly(Assembly assembly)
        {
            assembly.ExportedTypes
                .Where(x => x.GetTypeInfo().IsClass)
                .Where(x => !x.GetTypeInfo().IsAbstract)
                .Where(x => x.GetTypeInfo()
                    .ImplementedInterfaces
                    .Contains(typeof(IResourceMap)))
                .Each(Add);
        }

        public void Add(Type type)
        {
            var ctor = type.GetDefaultConstructor();

            var map = ctor.Invoke(null);

            var item = map as IResourceMap;
            if (item != null)
            {
                Maps.Add(item);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported mapper '{type.FullName}'");
            }
        }
    }
}