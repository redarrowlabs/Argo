using RedArrow.Jsorm.Cache;
using RedArrow.Jsorm.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace RedArrow.Jsorm.Config
{
    public class SessionConfiguration
    {
        //internal ICacheProvider CacheProvider { get; set; }
        internal Func<HttpClient> HttpClientFactory { get; set; }

        internal IDictionary<Type, string> Types { get; set; }
        internal IEnumerable<PropertyInfo> IdProperties { get; set; }
        internal IEnumerable<PropertyConfiguration> AttributeProperties { get; set; }

        internal SessionConfiguration()
        {
            //CacheProvider = new DefaultCacheProvider();
        }

        public ISessionFactory BuildSessionFactory()
        {
            var idLookup = IdProperties.ToDictionary(x => x.DeclaringType, x => x);
            var attributeLookup = AttributeProperties.ToLookup(x => x.PropertyInfo.DeclaringType, x => x);

            return new SessionFactory(
                HttpClientFactory,
                Types,
                idLookup,
                attributeLookup);
        }
    }
}