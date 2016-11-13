using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Collections.Generic;
using FieldAttributes = Mono.Cecil.FieldAttributes;

namespace RedArrow.Jsorm
{
	public class ModelWeavingContext
	{
		public Type ModelType { get; }
		public Type MapType { get; }
		public Type MapBaseType { get; }

		public PropertyDefinition IdPropDef { get; }

		public string MappedId { get; }
		public IDictionary<string, string> MappedAttributes { get; }
		public IDictionary<string, string> MappedHasOne { get; }
		public IDictionary<string, string> MappedHasMany { get; }

		public object Map { get; }

		private TypeDefinition ModelTypeDef { get; }
		public TypeReference ModelTypeRef => ModelTypeDef;
		private TypeDefinition MapTypeDef { get; }

		public Collection<FieldDefinition> Fields => ModelTypeDef.Fields;
		public Collection<MethodDefinition> Methods => ModelTypeDef.Methods;
		public Collection<PropertyDefinition> Properties => ModelTypeDef.Properties;  

		public TypeReference SessionTypeRef { get; private set; }
		public FieldDefinition SessionField { get; private set; }

		public ModelWeavingContext(TypeDefinition modelTypeDef, TypeDefinition mapTypeDef)
		{
			ModelTypeDef = modelTypeDef;
			MapTypeDef = mapTypeDef;

			ModelType = Type.GetType($"{modelTypeDef.FullName}, {modelTypeDef.Module.Assembly.FullName}");
			MapType = Type.GetType($"{mapTypeDef.FullName}, {mapTypeDef.Module.Assembly.FullName}");
			if (MapType != null)
			{
				Map = Activator.CreateInstance(MapType);
			}
			MapBaseType = GetMapBaseType(MapType);

			MappedId = GetMappedId(MapBaseType, Map);
			MappedAttributes = GetMappedProperties(MapBaseType, Map, "_attributeMaps");
			MappedHasOne = GetMappedProperties(MapBaseType, Map, "_referenceMaps");
			MappedHasMany = GetMappedProperties(MapBaseType, Map, "_collectionMaps");

			IdPropDef = Properties.FirstOrDefault(x => x.Name == MappedId);
		}

		public void AddSessionField(TypeDefinition sessionTypeDef)
		{
			if (SessionField != null) return;

			SessionTypeRef = ModelTypeDef.Module.ImportReference(sessionTypeDef);

			SessionField = new FieldDefinition(
					"__jsorm_generated_session",
					FieldAttributes.Private | FieldAttributes.NotSerialized | FieldAttributes.InitOnly,
					SessionTypeRef);
			
			Fields.Add(SessionField);
		}

		public TypeReference ImportReference(TypeDefinition typeDef)
		{
			return ModelTypeDef.Module.ImportReference(typeDef);
		}

		public TypeReference ImportReference(TypeReference typeRef)
		{
			return ModelTypeDef.Module.ImportReference(typeRef);
		}

		public MethodReference ImportReference(MethodDefinition methDef)
		{
			return ModelTypeDef.Module.ImportReference(methDef);
		}

		public MethodReference ImportReference(GenericInstanceMethod genMethDef)
		{
			return ModelTypeDef.Module.ImportReference(genMethDef);
		}

		private static string GetMappedId(Type mapType, object map)
		{
			var idMap = mapType
				.GetProperty("IdMap", BindingFlags.NonPublic | BindingFlags.Instance)
				?.GetValue(map);

			return idMap?.GetType()?.GetProperty("PropertyName", BindingFlags.NonPublic | BindingFlags.Instance)
				?.GetValue(idMap) as string;
		}

		private static IDictionary<string, string> GetMappedProperties(Type mapType, object map, string fieldName)
		{
			var dictionary = mapType
				.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
				?.GetValue(map); // IDictionary<string, IPropertyMap>

			var keys = dictionary?.GetType().GetProperty("Keys", BindingFlags.Public | BindingFlags.Instance)
				?.GetValue(dictionary) as IEnumerable<string>;

			return keys?.ToDictionary(key => key, key =>
			{
				var itemProp = dictionary.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
				var propMap = itemProp.GetValue(dictionary, new[] {key}); // Propertymap<>

				return propMap?.GetType().GetProperty("AttributeName", BindingFlags.NonPublic | BindingFlags.Instance)
					?.GetValue(propMap) as string;
			});
		}

		private static Type GetMapBaseType(Type mapType)
		{
			var ret = mapType.BaseType;

			while (ret != null && !ret.FullName.StartsWith("RedArrow.Jsorm.Map.ResourceMap`1"))
			{
				ret = ret.BaseType;
			}

			return ret;
		}
	}
}
