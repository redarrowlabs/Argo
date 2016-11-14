using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RedArrow.Jsorm.Extensions;
using RedArrow.Jsorm.Map.Attribute;
using RedArrow.Jsorm.Map.HasMany;
using RedArrow.Jsorm.Map.HasOne;
using RedArrow.Jsorm.Map.Id;
using RedArrow.Jsorm.Session;

namespace RedArrow.Jsorm.Map
{
    public class ResourceMap<TModel> : IResourceMap
        where TModel : class, new()
    {
        private IIdMap<TModel> IdMap { get; set; }

        private readonly IDictionary<string, IPropertyMap> _attributeMaps = new Dictionary<string, IPropertyMap>();

        private readonly IDictionary<string, IPropertyMap> _referenceMaps = new Dictionary<string, IPropertyMap>();

        private readonly IDictionary<string, IPropertyMap> _collectionMaps = new Dictionary<string, IPropertyMap>();

        public void Configure(SessionFactory factory)
        {
            factory.Register(typeof(TModel));

            IdMap.Configure(factory);
            _attributeMaps.Values.Each(x => x.Configure(factory));
            _referenceMaps.Values.Each(x => x.Configure(factory));
            _collectionMaps.Values.Each(x => x.Configure(factory));
        }

        protected IdMap<TModel> Id(Expression<Func<TModel, Guid>> id)
        {
            var idMap = new IdMap<TModel>(id);
            IdMap = idMap;
            return idMap;
        }

        protected AttributeMap<TModel, TProp> Attribute<TProp>(Expression<Func<TModel, TProp>> attribute, string attrName = null)
        {
            var propName = attribute.PropertyName();
            var attrMap = new AttributeMap<TModel, TProp>(attribute, attrName ?? propName);
            _attributeMaps[propName] = attrMap;
            return attrMap;
        }

        protected HasOneMap<TModel, TProp> HasOne<TProp>(Expression<Func<TModel, TProp>> toOne, string attrName = null)
            where TProp : new()
        {
            var propName = toOne.PropertyName();
            var toOneMap = new HasOneMap<TModel, TProp>(toOne, attrName ?? propName);
            _referenceMaps[propName] = toOneMap;
            return toOneMap;
        }

        protected IHasManyMap<TModel, TElement> HasMany<TElement>(Expression<Func<TModel, IEnumerable<TElement>>> toMany, string attrName = null)
            where TElement : new()
        {
            var propName = toMany.PropertyName();
            var toManyMap = new HasManyMap<TModel, TElement>(toMany, attrName ?? propName);
            _collectionMaps[propName] = toManyMap;
            return toManyMap;
        }
    }
}