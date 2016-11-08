﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using RedArrow.Jsorm.Core.Extensions;
using RedArrow.Jsorm.Core.Map.Attributes;
using RedArrow.Jsorm.Core.Map.HasMany;
using RedArrow.Jsorm.Core.Map.HasOne;
using RedArrow.Jsorm.Core.Map.Id;

namespace RedArrow.Jsorm.Core.Map
{
	public class ResourceMap<TModel> : IResourceMap
		where TModel : new()
	{
		private IIdMap IdMap { get; set; }

		private readonly IDictionary<string, IPropertyMap> _attributeMaps = new Dictionary<string, IPropertyMap>();
		
		private readonly IDictionary<string, IPropertyMap> _referenceMaps = new Dictionary<string, IPropertyMap>();

		private readonly IDictionary<string, IPropertyMap> _collectionMaps = new Dictionary<string, IPropertyMap>();

		protected IdMap<TModel, string> Id(Expression<Func<TModel, string>> id)
		{
			var idMap = new IdMap<TModel, string>(id);
			IdMap = idMap;
			return idMap;
		}

		protected IdMap<TModel, Guid> Id(Expression<Func<TModel, Guid>> id)
		{
			var idMap = new IdMap<TModel, Guid>(id);
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
