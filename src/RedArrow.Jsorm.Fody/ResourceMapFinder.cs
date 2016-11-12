using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace RedArrow.Jsorm
{
    public partial class ModuleWeaver
    {
        private IDictionary<TypeDefinition, TypeDefinition> _modelToMap;

        private void FindModels()
        {
            _modelToMap = ModuleDefinition.Types
                .Where(ExtendsResourceMap)
                .ToDictionary(GetMapModel, x => x);
        }

        private bool ExtendsResourceMap(TypeDefinition type)
        {
            return GetMapType(type) != null;
        }

        private TypeDefinition GetMapType(TypeDefinition type)
        {
            while (type != null)
            {
                if (type.BaseType != null
                    && type.BaseType.GetElementType().FullName == "RedArrow.Jsorm.Map.ResourceMap`1")
                {
                    break;
                }
                type = type.BaseType != null
                    ? ModuleDefinition.GetType(type.BaseType.FullName)
                    : null;
            }
            return type;
        }

        private TypeDefinition GetMapModel(TypeDefinition mapType)
        {
            var baseType = mapType.BaseType as GenericInstanceType;
            var modelTypeRef = baseType.GenericArguments.First();
            return ModuleDefinition.GetType(modelTypeRef.FullName);
        }
    }
}