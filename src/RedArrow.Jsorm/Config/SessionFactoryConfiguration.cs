using RedArrow.Jsorm.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace RedArrow.Jsorm.Config
{
    public class SessionFactoryConfiguration
    {
        //internal ICacheProvider CacheProvider { get; set; }
        internal Func<HttpClient> HttpClientFactory { get; set; }

        internal IDictionary<Type, string> Types { get; set; }
        internal IEnumerable<PropertyInfo> IdProperties { get; set; }
        internal IEnumerable<PropertyConfiguration> AttributeProperties { get; set; }
        internal IEnumerable<HasOneConfiguration> HasOneProperties { get; set; }
        internal IEnumerable<HasManyConfiguration> HasManyProperties { get; set; }

        internal SessionFactoryConfiguration()
        {
            //CacheProvider = new DefaultCacheProvider();
        }

        public ISessionFactory BuildSessionFactory()
        {
            var idLookup = IdProperties.ToDictionary(x => x.DeclaringType, x => x);
            var attributeLookup = AttributeProperties.ToLookup(x => x.PropertyInfo.DeclaringType, x => x);
            var hasOneLookup = HasOneProperties.ToLookup(x => x.PropertyInfo.DeclaringType, x => x);
            var hasManyLookup = HasManyProperties.ToLookup(x => x.PropertyInfo.DeclaringType, x => x);

            return new SessionFactory(
                HttpClientFactory,
                Types,
                idLookup,
                attributeLookup,
                hasOneLookup,
                hasManyLookup);
        }
    }
}