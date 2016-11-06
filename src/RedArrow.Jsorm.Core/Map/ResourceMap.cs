using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RedArrow.Jsorm.Core.Extensions;

namespace RedArrow.Jsorm.Core.Map
{
	public class ResourceMap<TModel> : IResourceMap
		where TModel : new()
	{
		public IIdMap IdMap { get; private set; }

		public IDictionary<string, IAttributeMap> AttributeMaps { get; } = new Dictionary<string, IAttributeMap>();
		
		public IDictionary<string, IHasOneMap> HasOneMaps { get; } = new Dictionary<string, IHasOneMap>();

		public IDictionary<string, IHasManyMap> HasManyMaps { get; } = new Dictionary<string, IHasManyMap>();

		protected IIdMap Id(Expression<Func<TModel, string>> id)
		{
			IdMap = new IdMap<TModel, string>(id);
			return IdMap;
		}

		protected IIdMap Id(Expression<Func<TModel, Guid>> id)
		{
			IdMap = new IdMap<TModel, Guid>(id);
			return IdMap;
		}

		protected IAttributeMap Attribute<TProp>(Expression<Func<TModel, TProp>> attribute, string attrName = null)
		{
			var propName = attribute.PropertyName();
			var attrMap = new AttributeMap<TModel, TProp>(attribute, attrName ?? propName);
			AttributeMaps[propName] = attrMap;
			return attrMap;
		}

		protected IHasOneMap HasOne<TProp>(Expression<Func<TModel, TProp>> toOne, string attrName = null)
			where TProp : new()
		{
			var propName = toOne.PropertyName();
			var toOneMap = new HasOneMap<TModel, TProp>(toOne, attrName ?? propName);
			HasOneMaps[propName] = toOneMap;
			return toOneMap;
		}

		protected IHasManyMap HasMany<TElement>(Expression<Func<TModel, IEnumerable<TElement>>> toMany, string attrName = null)
			where TElement : new()
		{
			var propName = toMany.PropertyName();
			var toManyMap = new HasManyMap<TModel, TElement>(toMany, attrName ?? propName);
			HasManyMaps[propName] = toManyMap;
			return toManyMap;
		}
	}
}
