using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace RedArrow.Jsorm
{
	public partial class ModuleWeaver
	{
		private void WeaveAttributes(TypeDefinition modelTypeDef, TypeDefinition mapTypeDef, FieldDefinition sessionFieldDef)
		{
			var sessionGetAttr = _sessionTypeRef.Methods.First(x => x.Name == "GetAttribute");
			
			var mapType = Type.GetType($"{mapTypeDef.FullName}, {mapTypeDef.Module.Assembly.FullName}");
			var mapBaseType = GetMapBaseType(mapType);

			if (mapType == null)
			{
				LogError($"Jsorm failed to load {mapTypeDef.FullName} from assembly {mapTypeDef.Module.Assembly.FullName}");
				return;
			}

			var map = Activator.CreateInstance(mapType);
			var attributes = GetMappedAttributes(mapBaseType, map);

			foreach (var attribute in attributes)
			{
				var propertyDef = modelTypeDef.Properties.SingleOrDefault(x => x.Name == attribute);
				if (propertyDef == null)
				{
					LogError($"Jsorm failed to weave mapped property {attribute} on model {modelTypeDef.FullName}");
					break;
				}

				var instructions = propertyDef.GetMethod.Body.Instructions;
				instructions.Insert(0, Instruction.Create(OpCodes.Ret));
				instructions.Insert(0, Instruction.Create(OpCodes.Castclass, propertyDef.GetMethod.ReturnType));
				instructions.Insert(0, Instruction.Create(OpCodes.Callvirt, modelTypeDef.Module.ImportReference(sessionGetAttr)));
				instructions.Insert(0, Instruction.Create(OpCodes.Ldfld, sessionFieldDef));
				instructions.Insert(0, Instruction.Create(OpCodes.Ldarg_0));
			}
		}

		private IEnumerable<string> GetMappedAttributes(Type mapType, object map)
		{
			var dictionary = mapType
				.GetField("_attributeMaps", BindingFlags.NonPublic | BindingFlags.Instance)
				?.GetValue(map); // IDictionary<string, IPropertymap>

			return dictionary?.GetType()?.GetProperty("Keys", BindingFlags.Public | BindingFlags.Instance)
				?.GetValue(dictionary) as IEnumerable<string>;
		}

		private Type GetMapBaseType(Type mapType)
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
